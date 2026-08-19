using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace gc.api.core.Servicios
{
    public class LinkServicio : Servicio<EntidadBase>, ILinkServicio
    {
        private readonly IConfiguration _configuration;
        public LinkServicio(IUnitOfWork uow, IConfiguration configuration) : base(uow)
        {
            _configuration = configuration;
        }
        public ReporteLinkResponseDto CrearLink(ReporteSolicitudDto solicitud, string usu_id, string? clienteId = null)
        {
            ValidarPoliticaEnlace(solicitud);

            var codigo = GenerarCodigoUnicoAsync();
            var minutosExpiracion = _configuration.GetValue<int>("Reportes:LinkExpiracionMinutos", 60);
            var maxDescargas = Math.Clamp(
                _configuration.GetValue<int>("Reportes:EnlacesPublicos:MaxDescargas", 5),
                1,
                50);
            var ventanaDescargaMinutos = Math.Clamp(
                _configuration.GetValue<int>("Reportes:EnlacesPublicos:VentanaDesdePrimerIntentoMinutos", 60),
                1,
                10080);
            var controlDescargasHabilitado = ControlDescargasHabilitado();

            var sp = ConstantesGC.StoredProcedures.SP_REPO_INSERTAR;

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@Codigo", codigo),
                new SqlParameter("@PayloadJson", JsonConvert.SerializeObject(solicitud)),
                new SqlParameter("@FechaCreacionUtc", DateTime.UtcNow),
                new SqlParameter("@FechaExpiracionUtc", DateTime.UtcNow.AddMinutes(minutosExpiracion)),
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@CreadoPor", usu_id),
                new SqlParameter("@Id", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output },
            };

            if (controlDescargasHabilitado)
            {
                ps.Insert(ps.Count - 1, new SqlParameter("@MaxDescargas", maxDescargas));
                ps.Insert(ps.Count - 1, new SqlParameter("@VentanaDescargaMinutos", ventanaDescargaMinutos));
            }

            var resultado = _repository.InvokarSpNQuery(sp, ps);
            var idGenerado = (long)ps.FirstOrDefault(p => p.ParameterName == "@Id").Value;
            if (idGenerado == 0 || idGenerado < 0)
                throw new Exception("Error al crear el link del reporte.");

            //la URL se construye en el controlador GestorImpresionController del Sitio Geco.
            return new ReporteLinkResponseDto
            {
                Codigo = codigo,
                ExpiraEnUtc = DateTime.UtcNow.AddMinutes(minutosExpiracion),
                MaxDescargas = maxDescargas,
                VentanaDescargaMinutos = ventanaDescargaMinutos
            };
        }


        public ReporteLinkAccesoResponseDto ObtenerSolicitud(
            string codigo,
            ReporteLinkAccesoContextoDto contexto)
        {
            var sp = ConstantesGC.StoredProcedures.SP_REPO_EXISTE;

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@Codigo", codigo)
            };

            //se verifica si existe el código, si no existe o ya fue usado o expiró, se retorna null. Si es válido, se marca como usado y se retorna la solicitud.


            var reg = _repository.InvokarSpScalar(sp, ps);


            if ((int)reg == 0)
            {
                throw new NegocioException("El código no existe");
            }

            //si existe, se obtiene la entidad completa para verificar si ya fue usado o expiró, y para marcarlo como usado.
            sp = ConstantesGC.StoredProcedures.SP_REPO_RESUELVE;

            ps = new List<SqlParameter>
                {
                    new SqlParameter("@Codigo", codigo)
                };

            if (ControlDescargasHabilitado())
            {
                ps.Add(new SqlParameter("@Ip", Limitar(contexto?.Ip, 45)));
                ps.Add(new SqlParameter("@UserAgent", Limitar(contexto?.UserAgent, 500)));
                ps.Add(new SqlParameter("@Referer", Limitar(contexto?.Referer, 1000)));
            }

            var entity = _repository.EjecutarLstSpExt<ReporteLinkDto>(sp, ps, true);

            if (entity == null || entity.Count == 0)
            {
                throw new NegocioException("Error al resolver el código");
            }

            int estado = entity[0].Estado;

            switch (estado)
            {
                case 1:
                    throw new NegocioException("El código no existe");
                case 2:
                    throw new NegocioException(ControlDescargasHabilitado()
                        ? "El enlace alcanzó el límite de descargas permitido"
                        : "El código ya fue usado");
                case 3:
                    throw new NegocioException("El código ha expirado");
                case 4:
                    throw new NegocioException("La ventana de descarga del enlace ha expirado");
                case 5:
                    throw new NegocioException("El enlace alcanzó el límite de descargas permitido");
                default:
                    break;
            }


            //if (entity[0].FechaExpiracionUtc < DateTime.UtcNow)
            //    throw new NegocioException("El código ha expirado");          

            var solicitud = JsonConvert.DeserializeObject<ReporteSolicitudDto>(entity[0].PayloadJson)
                ?? throw new NegocioException("La solicitud almacenada no posee un formato válido.");

            // La política también se evalúa al descargar. De este modo, un cambio
            // de configuración invalida enlaces sensibles emitidos con anterioridad.
            ValidarPoliticaEnlace(solicitud);

            if (ControlDescargasHabilitado()
                && (!entity[0].AccesoId.HasValue || entity[0].AccesoId.Value <= 0))
            {
                throw new NegocioException("No se pudo registrar el intento de descarga.");
            }

            return new ReporteLinkAccesoResponseDto
            {
                Solicitud = solicitud,
                AccesoId = entity[0].AccesoId ?? 0,
                MaxDescargas = entity[0].MaxDescargas,
                CantidadDescargas = entity[0].CantidadDescargas,
                FechaVentanaHastaUtc = entity[0].FechaVentanaHastaUtc
            };
        }

        public ReporteLinkOperacionResponseDto ConfirmarDescarga(ReporteLinkDescargaDto descarga)
        {
            if (!ControlDescargasHabilitado())
            {
                return new ReporteLinkOperacionResponseDto
                {
                    Estado = 0,
                    Mensaje = "Control de descargas no habilitado. Se conserva el comportamiento anterior."
                };
            }

            if (descarga == null || string.IsNullOrWhiteSpace(descarga.Codigo) || descarga.AccesoId <= 0)
            {
                throw new NegocioException("La confirmación de descarga es inválida.");
            }

            var ps = new List<SqlParameter>
            {
                new("@Codigo", descarga.Codigo),
                new("@AccesoId", descarga.AccesoId),
                new("@Bytes", (object?)descarga.Bytes ?? DBNull.Value),
                new("@DuracionMs", (object?)descarga.DuracionMs ?? DBNull.Value),
                new("@ResultadoHttp", (object?)descarga.ResultadoHttp ?? 200)
            };

            var resultado = _repository.EjecutarLstSpExt<ReporteLinkOperacionResponseDto>(
                ConstantesGC.StoredProcedures.SP_REPO_CONFIRMAR_DESCARGA,
                ps,
                true);

            if (resultado == null || resultado.Count == 0)
            {
                throw new NegocioException("No se pudo confirmar la descarga.");
            }

            if (resultado[0].Estado != 0)
            {
                throw new NegocioException(resultado[0].Mensaje);
            }

            return resultado[0];
        }

        public void RegistrarFallo(ReporteLinkDescargaDto descarga)
        {
            if (!ControlDescargasHabilitado())
            {
                return;
            }

            if (descarga == null || string.IsNullOrWhiteSpace(descarga.Codigo) || descarga.AccesoId <= 0)
            {
                return;
            }

            var ps = new List<SqlParameter>
            {
                new("@Codigo", descarga.Codigo),
                new("@AccesoId", descarga.AccesoId),
                new("@DuracionMs", (object?)descarga.DuracionMs ?? DBNull.Value),
                new("@ResultadoHttp", (object?)descarga.ResultadoHttp ?? 500),
                new("@Detalle", Limitar(descarga.Detalle, 500))
            };

            _repository.InvokarSpNQuery(
                ConstantesGC.StoredProcedures.SP_REPO_REGISTRAR_FALLO,
                ps);
        }


        private void ValidarPoliticaEnlace(ReporteSolicitudDto solicitud)
        {
            if (solicitud == null)
            {
                throw new NegocioException("La solicitud del reporte es requerida.");
            }

            const string seccion = "Reportes:EnlacesPublicos";
            if (!_configuration.GetValue($"{seccion}:Habilitado", true))
            {
                throw new NegocioException("La generación de enlaces públicos está deshabilitada.");
            }

            var reporteId = (int)solicitud.Reporte;
            var politica = $"{seccion}:PoliticasPorReporte:{reporteId}";
            var permitidoExplicito = _configuration.GetValue<bool?>($"{politica}:Permitido");
            var permitirNoConfigurados = _configuration.GetValue(
                $"{seccion}:PermitirNoConfigurados",
                true);
            var permitido = permitidoExplicito ?? permitirNoConfigurados;

            if (!permitido)
            {
                throw new NegocioException(
                    "El documento solicitado no está autorizado para descarga mediante enlace.");
            }

            var requiereAuditoria = _configuration.GetValue(
                $"{politica}:RequiereAuditoria",
                false);
            var auditoriaDisponible = _configuration.GetValue(
                $"{seccion}:AuditoriaDisponible",
                false);

            if (requiereAuditoria && !auditoriaDisponible)
            {
                throw new NegocioException(
                    "El documento requiere auditoría reforzada y aún no está habilitado para enlaces públicos.");
            }
        }

        private string GenerarCodigoUnicoAsync()
        {
            var sp = ConstantesGC.StoredProcedures.SP_REPO_EXISTE;

            while (true)
            {
                var codigo = GenerarCodigoCorto(8);
                var ps = new List<SqlParameter>
                {
                    new SqlParameter("@Codigo", codigo)
                };
                var existe = _repository.InvokarSpScalar(sp, ps);

                if ((int)existe == 0)
                    return codigo;
            }
        }

        private static string GenerarCodigoCorto(int longitud)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            var bytes = RandomNumberGenerator.GetBytes(longitud);
            var result = new char[longitud];

            for (int i = 0; i < longitud; i++)
                result[i] = chars[bytes[i] % chars.Length];

            return new string(result);
        }

        private static object Limitar(string? valor, int longitudMaxima)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return DBNull.Value;
            }

            var limpio = valor.Trim();
            return limpio.Length <= longitudMaxima
                ? limpio
                : limpio[..longitudMaxima];
        }

        private bool ControlDescargasHabilitado()
        {
            return _configuration.GetValue(
                "Reportes:EnlacesPublicos:ControlDescargasHabilitado",
                false);
        }

    }
}
