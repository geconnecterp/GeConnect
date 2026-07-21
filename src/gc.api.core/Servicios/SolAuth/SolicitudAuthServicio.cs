using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.SolAuth;
using gc.api.core.Entidades;
using gc.api.core.Entidades.SAuth;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.SolAuth.Comando;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.Enumeraciones;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using RemoteAuthorizations.Application.Responses;
using System.Diagnostics;

namespace gc.api.core.Servicios.SolAuth
{
    public class SolicitudAuthServicio:Servicio<EntidadBase>, ISolicitudAuthServicio
    {
        private const string CodigoResolucionPosesionDerecho = "POSESION_DERECHO";
        private readonly IRepository<ResolucionAutorizacion> _resolucionRepo;
        private readonly IRepository<MensajeBandejaSalida> _outboxRepo;

        private readonly ILoggerHelper _logger;
        public SolicitudAuthServicio(IUnitOfWork uow, ILoggerHelper logger) : base(uow)
        {
            _logger = logger;
            _resolucionRepo = uow.GetRepository<ResolucionAutorizacion>();
            _outboxRepo = uow.GetRepository<MensajeBandejaSalida>();
        }


        public async Task<SolicitudAutorizacionRespuesta> CrearAsync(
       CrearSolicitudAutorizacionComando comando,
       string idempotencyKey,
       string codigoModuloOrigen,
       CancellationToken cancellationToken = default)
        {
            var decisionPorDefecto = Enum.Parse<DecisionAutorizacion>(comando.ResolucionPorDefecto.Decision, ignoreCase: true);
            var contextoJson = comando.Contexto?.ToString(Newtonsoft.Json.Formatting.None) ?? "{}";

            var solicitud = new SolicitudAutorizacion(
                codigoModuloOrigen,
                comando.usu_id,
                comando.DerCodigo,
                comando.IdSolicitudExterna,
                comando.TimeoutSegundos,
                decisionPorDefecto,
                comando.ResolucionPorDefecto.CodigoResolucion,
                contextoJson,
                idempotencyKey);

            var sp = ConstantesGC.StoredProcedures.SP_AUTH_SOLICITUD_AUTORIZACION_CREAR;
            var dict = new List<SqlParameter>
        {
            new("@Id", solicitud.Id),
            new("@IdSolicitudExterna", solicitud.IdSolicitudExterna),
            new("@DerCodigo", solicitud.DerCodigo),
            new("@Estado", solicitud.Estado.ToString()),
            new("@IdUsuarioSolicitante", solicitud.IdUsuarioSolicitante),
            new("@CodigoModuloOrigen", solicitud.CodigoModuloOrigen),
            new("@FechaSolicitud", solicitud.FechaSolicitud),
            new("@TimeoutSegundos", solicitud.TimeoutSegundos),
            new("@FechaExpiracion", solicitud.FechaExpiracion),
            new("@DecisionPorDefecto", solicitud.DecisionPorDefecto.ToString()),
            new("@CodigoResolucionPorDefecto", solicitud.CodigoResolucionPorDefecto),
            new("@MensajeResolucionPorDefecto", comando.ResolucionPorDefecto.Mensaje ?? (object)DBNull.Value),
            new("@ContextoJson", solicitud.ContextoJson),
            new("@IdempotencyKey", solicitud.IdempotencyKey)
        };

            var solicitantePoseeDerecho = UsuarioSolicitantePoseeDerecho(
                solicitud.IdUsuarioSolicitante,
                solicitud.DerCodigo);

            _uow.InicializarTransaccion();

            try
            {
                _repository.InvokarSpNQuery(sp, dict, true, false);

                InsertarEventoCreada(solicitud);

                if (solicitantePoseeDerecho)
                {
                    solicitud.ResolverAutomaticamentePorPosesionDerecho();
                    _logger.Log(
                        TraceEventType.Information,
                        $"Solicitud {solicitud.Id}: se detecto posesion del derecho {solicitud.DerCodigo}. Persistiendo autorizacion automatica.");

                    PersistirResolucion(solicitud);
                    InsertarEventoResuelta(solicitud);

                    _logger.Log(
                        TraceEventType.Information,
                        $"Solicitud {solicitud.Id} autorizada automaticamente por posesion del derecho {solicitud.DerCodigo}.");
                }
                else
                {
                    _logger.Log(
                        TraceEventType.Information,
                        $"Solicitud {solicitud.Id}: el usuario {solicitud.IdUsuarioSolicitante} no posee el derecho {solicitud.DerCodigo}; queda pendiente de autorizacion remota.");
                }

                _uow.Commit();
            }
            catch (Exception ex)
            {
                try
                {
                    _uow.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    _logger.Log(rollbackEx);
                    _logger.Log(
                        TraceEventType.Error,
                        $"No se pudo deshacer la transaccion de la solicitud {solicitud.Id}. Excepcion original: {ex.Message}");
                }

                throw;
            }

            return await Task.FromResult(MapToResponse(solicitud));
        }

        public async Task BloquearAsync(Guid idSolicitud, string idUsuario, CancellationToken cancellationToken = default)
        {
            var sp = ConstantesGC.StoredProcedures.SP_AUTH_SOLICITUD_AUTORIZACION_OBTENER;
            var dictGet = new List<SqlParameter> {
            new("@IdSolicitud", idSolicitud),
            new("@IdUsuario", idUsuario)
        };
            var solicitudes = _repository.EjecutarLstSpExt<SolicitudAutorizacion>(sp, dictGet);
            var solicitud = solicitudes.FirstOrDefault();

            if (solicitud == null)
            {
                throw new Exception("Solicitud no encontrada.");
            }

            ValidarQueNoSeaElSolicitante(solicitud, idUsuario);

            if (!solicitud.PuedeAutorizar)
            {
                throw new Exception("No tiene permisos para autorizar esta categoría.");
            }

            solicitud.Bloquear(idUsuario);

            sp = ConstantesGC.StoredProcedures.SP_AUTH_SOLICITUD_AUTORIZACION_BLOQUEAR;
            var rowsAffected = new SqlParameter("@RowsAffected", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            var dictLock = new List<SqlParameter>
        {
            new("@IdSolicitud", solicitud.Id),
            new("@IdUsuario", solicitud.IdUsuarioBloqueo!),
            rowsAffected
        };

            _repository.InvokarSpNQuery(sp, dictLock);

            if (rowsAffected.Value == DBNull.Value || Convert.ToInt32(rowsAffected.Value) == 0)
            {
                throw new InvalidOperationException(
                    "La solicitud fue tomada por otro usuario antes de completar el bloqueo.");
            }
        }

        public async Task<ResolucionAutorizacionRespuesta> ResolverAsync(
            Guid idSolicitud,
            ResolverSolicitudAutorizacionComando comando,
            string idempotencyKey,
            string idUsuarioResolucion,
            CancellationToken cancellationToken = default)
        {
            var sp = ConstantesGC.StoredProcedures.SP_AUTH_SOLICITUD_AUTORIZACION_OBTENER;

            var dictGet = new List<SqlParameter> {
                new("@IdSolicitud", idSolicitud),
                new("@IdUsuario", idUsuarioResolucion)
            };
            var solicitudes = _repository.EjecutarLstSpExt<SolicitudAutorizacion>(sp, dictGet);

            var solicitud = solicitudes.FirstOrDefault();

            if (solicitud == null)
            {
                throw new Exception("Solicitud no encontrada.");
            }

            ValidarQueNoSeaElSolicitante(solicitud, idUsuarioResolucion);

            if (solicitud.Estado != EstadoAutorizacion.PENDIENTE && solicitud.Estado != EstadoAutorizacion.EN_PROCESO)
            {
                throw new Exception("La solicitud ya fue resuelta o ha vencido.");
            }

            if (!solicitud.PuedeAutorizar)
            {
                throw new Exception("No tiene permisos para autorizar esta categoría.");
            }

            var decision = Enum.Parse<DecisionAutorizacion>(comando.Decision, ignoreCase: true);

            solicitud.Resolver(
                decision,
                comando.CodigoResolucion,
                comando.Mensaje,
                idUsuarioResolucion,
                false);

            _uow.InicializarTransaccion();

            try
            {
                sp = ConstantesGC.StoredProcedures.SP_AUTH_SOLICITUD_AUTORIZACION_RESOLVER;

                var res = solicitud.Resolucion!;
                var dictRes = new List<SqlParameter>
            {
                new("@IdResolucion", res.Id),
                new("@IdSolicitud", res.IdSolicitud),
                new("@Decision", res.Decision.ToString()),
                new("@CodigoResolucion", res.CodigoResolucion),
                new("@Mensaje", res.Mensaje ?? (object)DBNull.Value),
                new("@IdUsuarioResolucion", res.IdUsuarioResolucion),
                new("@EsResolucionPorDefecto", res.EsResolucionPorDefecto)
            };
                _resolucionRepo.InvokarSpNQuery(sp, dictRes, true, false);

                var resolutionEvent = new
                {
                    EventId = Guid.NewGuid(),
                    EventType = "SolicitudAutorizacionResuelta",
                    IdSolicitud = solicitud.Id,
                    CodigoModuloOrigen = solicitud.CodigoModuloOrigen,
                    IdSolicitudExterna = solicitud.IdSolicitudExterna,
                    Estado = solicitud.Estado.ToString(),
                    FechaResolucion = solicitud.Resolucion!.FechaResolucion
                };

                var outboxMessage = new MensajeBandejaSalida
                {
                    Id = Guid.NewGuid(),
                    Tipo = "SolicitudAutorizacionResuelta",
                    FechaOcurrencia = DateTime.UtcNow,
                    PayloadJson = JsonConvert.SerializeObject(resolutionEvent)
                };

                sp = ConstantesGC.StoredProcedures.SP_SAUTH_BANDEJA_SALIDA_INSERTAR;
                var dictOutbox = new List<SqlParameter>
            {
                new("@Id", outboxMessage.Id),
                new("@Tipo", outboxMessage.Tipo),
                new("@PayloadJson", outboxMessage.PayloadJson),
                new("@FechaOcurrencia", outboxMessage.FechaOcurrencia),
                new SqlParameter("@Intentos", System.Data.SqlDbType.Int) { Value = 0 }
            };
                _outboxRepo.InvokarSpNQuery(sp, dictOutbox, true, false);

                _uow.Commit();
            }
            catch (Exception)
            {
                _uow.Rollback();
                throw;
            }

            return await Task.FromResult(MapToResolutionResponse(solicitud.Resolucion!));
        }

        public async Task<SolicitudAutorizacionRespuesta> ObtenerResolucionAsync(Guid idSolicitud, string idUsuario, CancellationToken cancellationToken = default)
        {
            var sp = ConstantesGC.StoredProcedures.SP_AUTH_SOLICITUD_AUTORIZACION_OBTENER;

            var dictGet = new List<SqlParameter>
            {
                new("@IdSolicitud", idSolicitud),
                new("@IdUsuario", idUsuario)
            };
            var resultados = _repository.EjecutarSpDosResultados<
                SolicitudAutorizacion,
                ResolucionAutorizacion>(sp, dictGet, true);

            var solicitud = resultados.Primero.FirstOrDefault();

            if (solicitud == null)
            {
                throw new Exception("Solicitud no encontrada.");
            }

            var respuesta = MapToResponse(solicitud);
            var resolucion = resultados.Segundo.FirstOrDefault();
            if (resolucion is not null)
            {
                respuesta.Resolucion = MapToResolutionResponse(resolucion);
            }

            if (solicitud.Estado is EstadoAutorizacion.RESUELTO or EstadoAutorizacion.EXPIRADO &&
                respuesta.Resolucion is null)
            {
                var excepcion = new InvalidOperationException(
                    $"La solicitud {solicitud.Id} está en estado {solicitud.Estado} sin una resolución persistida.");
                _logger.Log(excepcion);
                throw excepcion;
            }

            return await Task.FromResult(respuesta);
        }

        public async Task<IEnumerable<SolicitudAutorizacionRespuesta>> ObtenerPendientesAsync(string idUsuario, CancellationToken cancellationToken = default)
        {
            var sp = ConstantesGC.StoredProcedures.SP_AUTH_SOLICITUD_AUTORIZACION_PENDIENTES;

            var dictGet = new List<SqlParameter> { new("@IdUsuario", idUsuario) };
            var solicitudes = _repository.EjecutarLstSpExt<SolicitudAutorizacion>(sp, dictGet);
            var responses = solicitudes.Select(MapToResponse).ToList();
            return await Task.FromResult(responses);
        }

        public async Task<IEnumerable<SolicitudAutorizacionRespuesta>> ObtenerHistoricoAsync(DateTime fechaDesde, DateTime fechaHasta, int top, string idUsuario, CancellationToken cancellationToken = default)
        {
            var sp = ConstantesGC.StoredProcedures.SP_AUTH_SOLICITUD_AUTORIZACION_HISTORICO;

            var dictSols = new List<SqlParameter>
        {
            new("@FechaDesde", fechaDesde),
            new("@FechaHasta", fechaHasta),
            new("@Top", top),
            new("@IdUsuario", idUsuario)
        };
            var solicitudes = _repository.EjecutarLstSpExt<SolicitudAutorizacion>(sp, dictSols);

            sp = ConstantesGC.StoredProcedures.SP_AUTH_RESOLUCION_AUTORIZACION_HISTORICO;
            var dictRes = new List<SqlParameter>
        {
            new("@FechaDesde", fechaDesde),
            new("@FechaHasta", fechaHasta)
        };
            var resoluciones = _resolucionRepo.EjecutarLstSpExt<ResolucionAutorizacion>(sp, dictRes);

            var responses = new List<SolicitudAutorizacionRespuesta>();
            foreach (var sol in solicitudes)
            {
                var resDto = MapToResponse(sol);
                var resEnt = resoluciones.FirstOrDefault(r => r.IdSolicitud == sol.Id);
                if (resEnt != null)
                {
                    resDto.Resolucion = MapToResolutionResponse(resEnt);
                }
                responses.Add(resDto);
            }

            return await Task.FromResult(responses);
        }

        public async Task ExpirarSolicitudesPendientesAsync(CancellationToken cancellationToken = default)
        {
            var sp = ConstantesGC.StoredProcedures.SP_AUTH_SOLICITUD_AUTORIZACION_OBTENER_EXPIRADAS;
            var expiradas = _repository.EjecutarLstSpExt<SolicitudAutorizacion>(sp, []);

            foreach (var solicitud in expiradas)
            {
                try
                {
                    solicitud.Expirar();

                    _uow.InicializarTransaccion();

                    var res = solicitud.Resolucion!;

                sp= ConstantesGC.StoredProcedures.SP_AUTH_SOLICITUD_AUTORIZACION_EXPIRAR;
                    var dictRes = new List<SqlParameter>
                {
                    new("@IdResolucion", res.Id),
                    new("@IdSolicitud", res.IdSolicitud),
                    new("@Decision", res.Decision.ToString()),
                    new("@CodigoResolucion", res.CodigoResolucion),
                    new("@Mensaje", res.Mensaje ?? (object)DBNull.Value),
                    new("@IdUsuarioResolucion", res.IdUsuarioResolucion),
                    new("@EsResolucionPorDefecto", res.EsResolucionPorDefecto)
                };

                    _resolucionRepo.InvokarSpNQuery(sp, dictRes, true, false);

                    var resolutionEvent = new
                    {
                        EventId = Guid.NewGuid(),
                        EventType = "SolicitudAutorizacionResuelta",
                        IdSolicitud = solicitud.Id,
                        CodigoModuloOrigen = solicitud.CodigoModuloOrigen,
                        IdSolicitudExterna = solicitud.IdSolicitudExterna,
                        Estado = solicitud.Estado.ToString(),
                        FechaResolucion = solicitud.Resolucion!.FechaResolucion
                    };

                    var outboxMessage = new MensajeBandejaSalida
                    {
                        Id = Guid.NewGuid(),
                        Tipo = "SolicitudAutorizacionResuelta",
                        FechaOcurrencia = DateTime.UtcNow,
                        PayloadJson = JsonConvert.SerializeObject(resolutionEvent)
                    };

                    sp = ConstantesGC.StoredProcedures.SP_SAUTH_BANDEJA_SALIDA_INSERTAR;
                    var dictOutbox = new List<SqlParameter>
                {
                    new("@Id", outboxMessage.Id),
                    new("@Tipo", outboxMessage.Tipo),
                    new("@PayloadJson", outboxMessage.PayloadJson),
                    new("@FechaOcurrencia", outboxMessage.FechaOcurrencia),
                    new SqlParameter("@Intentos", System.Data.SqlDbType.Int) { Value = 0 }
                };
                    _outboxRepo.InvokarSpNQuery(sp, dictOutbox, true, false);

                    _uow.Commit();
                }
                catch (Exception ex)
                {
                    _uow.Rollback();
                    // Continuar con la siguiente aunque una falle
                    Console.WriteLine($"Error expirando solicitud {solicitud.Id}: {ex.Message}");
                }
            }
        }

        private static SolicitudAutorizacionRespuesta MapToResponse(SolicitudAutorizacion solicitud)
        {
            return new SolicitudAutorizacionRespuesta
            {
                Id = solicitud.Id,
                Estado = solicitud.Estado.ToString(),
                CodigoModuloOrigen = solicitud.CodigoModuloOrigen,
                IdUsuarioSolicitante = solicitud.IdUsuarioSolicitante,
                DerCodigo = solicitud.DerCodigo,
                DerechoDescripcion = solicitud.DerechoDescripcion,
                FechaSolicitud = solicitud.FechaSolicitud,
                TimeoutSegundos = solicitud.TimeoutSegundos,
                FechaExpiracion = solicitud.FechaExpiracion,
                IdUsuarioBloqueo = solicitud.IdUsuarioBloqueo,
                FechaBloqueo = solicitud.FechaBloqueo,
                ContextoJson = solicitud.ContextoJson,
                IdSolicitudExterna = solicitud.IdSolicitudExterna,
                PuedeAutorizar = solicitud.PuedeAutorizar,
                Resolucion = solicitud.Resolucion != null ? MapToResolutionResponse(solicitud.Resolucion) : null
            };
        }

        private static ResolucionAutorizacionRespuesta MapToResolutionResponse(ResolucionAutorizacion resolucion)
        {
            return new ResolucionAutorizacionRespuesta
            {
                Id = resolucion.Id,
                IdSolicitud = resolucion.IdSolicitud,
                Decision = resolucion.Decision.ToString(),
                CodigoResolucion = resolucion.CodigoResolucion,
                Mensaje = resolucion.Mensaje,
                FechaResolucion = resolucion.FechaResolucion,
                IdUsuarioResolucion = resolucion.IdUsuarioResolucion,
                EsResolucionPorDefecto = resolucion.EsResolucionPorDefecto
            };
        }

        private static void ValidarQueNoSeaElSolicitante(
            SolicitudAutorizacion solicitud,
            string idUsuarioAutorizador)
        {
            if (string.Equals(
                solicitud.IdUsuarioSolicitante?.Trim(),
                idUsuarioAutorizador?.Trim(),
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                "El usuario solicitante no puede autorizar su propia solicitud.");
            }
        }

        private bool UsuarioSolicitantePoseeDerecho(string usuario, int derCodigo)
        {
            if (string.IsNullOrWhiteSpace(usuario) || derCodigo <= 0)
            {
                return false;
            }

            var sp = ConstantesGC.StoredProcedures.SP_USU_DER;
            var ps = new List<SqlParameter> { new("@usu_id", usuario) };
            try
            {
                var derechos = _repository.EjecutarLstSpExt<DerUserDto>(sp, ps, true);

                return derechos.Any(d =>
                    d.asignado &&
                    int.TryParse(d.der_codigo, out var codigo) &&
                    codigo == derCodigo);
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                _logger.Log(
                    TraceEventType.Error,
                    $"No se pudo verificar si el usuario {usuario} posee el derecho {derCodigo}.");
                throw;
            }
        }

        private void PersistirResolucion(SolicitudAutorizacion solicitud)
        {
            var resolucion = solicitud.Resolucion
                ?? throw new InvalidOperationException("La solicitud no contiene resolucion para persistir.");

            var sp = ConstantesGC.StoredProcedures.SP_AUTH_SOLICITUD_AUTORIZACION_RESOLVER;
            var dictRes = new List<SqlParameter>
            {
                new("@IdResolucion", resolucion.Id),
                new("@IdSolicitud", resolucion.IdSolicitud),
                new("@Decision", resolucion.Decision.ToString()),
                new("@CodigoResolucion", resolucion.CodigoResolucion),
                new("@Mensaje", resolucion.Mensaje ?? (object)DBNull.Value),
                new("@IdUsuarioResolucion", resolucion.IdUsuarioResolucion),
                new("@EsResolucionPorDefecto", resolucion.EsResolucionPorDefecto)
            };

            try
            {
                _resolucionRepo.InvokarSpNQuery(sp, dictRes, true, false);
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                _logger.Log(
                    TraceEventType.Error,
                    $"No se pudo persistir la autorizacion automatica de la solicitud {solicitud.Id} con codigo {CodigoResolucionPosesionDerecho}.");
                throw;
            }
        }

        private void InsertarEventoCreada(SolicitudAutorizacion solicitud)
        {
            var createdEvent = new
            {
                EventId = Guid.NewGuid(),
                EventType = "SolicitudAutorizacionCreada",
                IdSolicitud = solicitud.Id,
                CodigoModuloOrigen = solicitud.CodigoModuloOrigen,
                DerCodigo = solicitud.DerCodigo,
                Estado = EstadoAutorizacion.PENDIENTE.ToString(),
                FechaSolicitud = solicitud.FechaSolicitud
            };

            InsertarOutbox("SolicitudAutorizacionCreada", createdEvent, 1);
        }

        private void InsertarEventoResuelta(SolicitudAutorizacion solicitud)
        {
            var resolutionEvent = new
            {
                EventId = Guid.NewGuid(),
                EventType = "SolicitudAutorizacionResuelta",
                IdSolicitud = solicitud.Id,
                CodigoModuloOrigen = solicitud.CodigoModuloOrigen,
                IdSolicitudExterna = solicitud.IdSolicitudExterna,
                Estado = solicitud.Estado.ToString(),
                FechaResolucion = solicitud.Resolucion!.FechaResolucion,
                CodigoResolucion = CodigoResolucionPosesionDerecho,
                AutorizacionAutomatica = true
            };

            InsertarOutbox("SolicitudAutorizacionResuelta", resolutionEvent, 0);
        }

        private void InsertarOutbox(string tipo, object payload, int intentos)
        {
            var outboxMessage = new MensajeBandejaSalida
            {
                Id = Guid.NewGuid(),
                Tipo = tipo,
                FechaOcurrencia = DateTime.UtcNow,
                PayloadJson = JsonConvert.SerializeObject(payload)
            };

            var sp = ConstantesGC.StoredProcedures.SP_SAUTH_BANDEJA_SALIDA_INSERTAR;
            var dictOutbox = new List<SqlParameter>
            {
                new("@Id", outboxMessage.Id),
                new("@Tipo", outboxMessage.Tipo),
                new("@PayloadJson", outboxMessage.PayloadJson),
                new("@FechaOcurrencia", outboxMessage.FechaOcurrencia),
                new SqlParameter("@Intentos", System.Data.SqlDbType.Int) { Value = intentos }
            };

            _outboxRepo.InvokarSpNQuery(sp, dictOutbox, true, false);
        }
    }
}
