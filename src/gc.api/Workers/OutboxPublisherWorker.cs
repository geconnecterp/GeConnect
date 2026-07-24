using gc.api.core.Constantes;
using gc.api.core.Entidades.SAuth;
using gc.api.core.Interfaces.Datos;
using gc.api.Hubs;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;


namespace gc.api.Workers
{
    public class OutboxPublisherWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<AutorizacionHub> _hubContext;
        private readonly ILogger<OutboxPublisherWorker> _logger;
        private readonly PaginationOptions _paginacion;
        private static readonly TimeSpan IntervaloLogSinPendientes = TimeSpan.FromMinutes(1);
        private DateTime _ultimoLogSinPendientesUtc = DateTime.MinValue;

        public OutboxPublisherWorker(
            IServiceProvider serviceProvider,
            IHubContext<AutorizacionHub> hubContext,
            ILogger<OutboxPublisherWorker> logger,
            IOptions<PaginationOptions> paginacion
            )
        {
            
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logger = logger;
            _paginacion = paginacion.Value;

            _logger.LogInformation("Inicializando OutboxPublisherWorker");
        }


        private void RegistrarResultadoConsultaPendientes(int cantidad)
        {
            if (cantidad > 0)
            {
                _logger.LogInformation(
                    "Se encontraron {Count} mensajes pendientes en la bandeja de salida",
                    cantidad
                );
                return;
            }

            var ahoraUtc = DateTime.UtcNow;
            if (ahoraUtc - _ultimoLogSinPendientesUtc < IntervaloLogSinPendientes)
            {
                _logger.LogDebug("No hay mensajes pendientes en la bandeja de salida");
                return;
            }

            _ultimoLogSinPendientesUtc = ahoraUtc;
            _logger.LogInformation(
                "No hay mensajes pendientes en la bandeja de salida. Proxima verificacion informativa en {Minutos} minuto(s).",
                IntervaloLogSinPendientes.TotalMinutes
            );
        }

        /// <summary>
        /// Este método es el bucle principal de ejecución del OutboxPublisherWorker. Continuamente verifica si hay mensajes pendientes en la bandeja de salida e intenta publicarlos a través de SignalR. Si un mensaje se publica correctamente, actualiza su estado en la base de datos. Si ocurre un error durante la publicación o actualización, registra el error y continúa procesando otros mensajes.
        /// dentro del codigo loguear todas las partes del codigo
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Iniciando bucle principal de OutboxPublisherWorker");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Verificando mensajes pendientes en la bandeja de salida");
                    using var scope = _serviceProvider.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var outboxRepo = unitOfWork.GetRepository<MensajeBandejaSalida>();

                    _logger.LogDebug("Obteniendo mensajes pendientes de la bandeja de salida con un tamaño de lote de {LimiteAvalancha}", _paginacion.DefaultPageSize);
                    var dictGet = new List<SqlParameter> { new("@BatchSize", _paginacion.DefaultPageSize) };

                    _logger.LogDebug("Ejecutando procedimiento almacenado {StoredProcedure} para obtener mensajes pendientes", ConstantesGC.StoredProcedures.SP_SAUTH_BANDEJA_SALIDA_OBTENER_PENDIENTES);
                    var messages = outboxRepo.EjecutarLstSpExt<MensajeBandejaSalida>(ConstantesGC.StoredProcedures.SP_SAUTH_BANDEJA_SALIDA_OBTENER_PENDIENTES, dictGet);

                    RegistrarResultadoConsultaPendientes(messages.Count);
                    foreach (var message in messages)
                    {
                        message.Intentos++;
                        try
                        {
                            // Emitir evento por SignalR
                            await _hubContext.Clients.All.SendAsync(
                                "EventoAutorizacionRecibido",
                                message.PayloadJson,
                                cancellationToken: stoppingToken);

                            _logger.LogInformation("MensajeBandejaSalida {Id} ({Tipo}) emitido por SignalR", message.Id, message.Tipo);

                            message.FechaProcesado = DateTime.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            message.Error = ex.Message;
                            _logger.LogError(ex, "Falló la emisión del MensajeBandejaSalida {Id}", message.Id);
                        }

                        // Update outbox status
                        try
                        {
                            var dictUpd = new List<SqlParameter>
                        {
                            new("@Id", message.Id),
                            new("@FechaProcesado", message.FechaProcesado ?? (object)DBNull.Value),
                            new("@Intentos", message.Intentos),
                            new("@Error", message.Error ?? (object)DBNull.Value)
                        };
                            outboxRepo.InvokarSpNQuery(ConstantesGC.StoredProcedures.SP_SAUTH_BANDEJA_SALIDA_ACTUALIZAR, dictUpd, false);
                        }
                        catch (Exception updateEx)
                        {
                            _logger.LogError(updateEx, "Error actualizando estado del MensajeBandejaSalida {Id}", message.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en OutboxPublisherWorker");
                }
                
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}


