using gc.caja.Controllers;
using gc.infraestructura.Dtos.Cajas;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class PDiferidoController : ControladorBaseCaja
    {
        private readonly string Co_TipoCC;
        private readonly IFactDiferidaServicio _fdiferidoSv;
        private const string MODULO = "CobranzaDiferida";
        private const string MODULO_DESC = "Módulo de Cobranza Diferida";

        public PDiferidoController(IOptions<AppSettings> options, IHttpContextAccessor contexto,
            ILogger<PDiferidoController> logger, IFactDiferidaServicio fdiferidoSv) : base(options, contexto, logger)
        {
            Co_TipoCC = "CD";
            _fdiferidoSv = fdiferidoSv;
        }

        /// <summary>
        /// ✅ ACTUALIZADO v3.0: Vista principal del módulo.
        /// Carga todas las facturas pendientes al inicio y las envía a la vista.
        /// </summary>
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Index()
        {
            _logger?.LogInformation("═══════════════════════════════════════════════════");
            _logger?.LogInformation("🚀 INICIANDO MÓDULO DE COBRANZA DIFERIDA v3.0");
            _logger?.LogInformation("═══════════════════════════════════════════════════");

            // ❶ VALIDAR AUTENTICACIÓN
            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                _logger?.LogWarning("❌ Usuario no autenticado, redirigiendo a login");
                return redirectResult;
            }

            // ❷ CARGAR TODAS LAS FACTURAS PENDIENTES
            var resultado = await CargarTodasLasFacturasPendientes();

            // ❸ PREPARAR DATOS PARA LA VISTA
            ViewBag.FacturasPendientes = resultado.Facturas;
            ViewBag.TieneFacturas = resultado.Facturas?.Count > 0;
            ViewBag.MensajeError = resultado.MensajeError;
            ViewBag.HuboError = !string.IsNullOrEmpty(resultado.MensajeError);

            _logger?.LogInformation("═══════════════════════════════════════════════════");
            _logger?.LogInformation($"✅ Vista cargada con {resultado.Facturas?.Count ?? 0} facturas");
            _logger?.LogInformation("═══════════════════════════════════════════════════");

            ViewBag.Co_TipoCD = Co_TipoCC;
            ViewBag.ModuloCD = MODULO;
            ViewBag.ModuloDesc = MODULO_DESC;

            return View();
        }

        [HttpPost]
        public IActionResult Validar()
        {
            // TODO: Implementar lógica de validación de acceso al módulo de Cobranza Diferida.
            // Por ejemplo, verificar permisos del usuario, estado de la caja, etc.
            // Por ahora, se asume que la validación es siempre exitosa.

            return Json(new { success = true, message = "Acceso permitido" });
        }

        /// <summary>
        /// ✅ NUEVO v3.0: Obtiene facturas de un cliente específico desde la sesión (FILTRADO LOCAL).
        /// Este método NO hace llamadas al servicio, solo filtra los datos ya cargados en sesión.
        /// </summary>
        /// <param name="clienteId">ID del cliente a filtrar</param>
        /// <returns>Lista de facturas del cliente específico</returns>
        [HttpPost]
        public JsonResult ObtenerFacturasClienteDesdesesion([FromBody] string? clienteId)
        {
            try
            {
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🔍 FILTRAR FACTURAS POR CLIENTE (DESDE SESIÓN) v3.0");
                _logger?.LogInformation($"   Cliente solicitado: {clienteId}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                if (!VerificarAutenticacion(out _))
                {
                    return Json(new { ok = false, mensaje = "Sesión expirada." });
                }

                var cliente = ClienteActual;
                var consumidorFinal = string.Equals(cliente?.Origen?.Trim(), "F", StringComparison.OrdinalIgnoreCase);
                var documento = cliente?.cta_documento?.Trim() ?? string.Empty;
                if (cliente == null || (consumidorFinal ? string.IsNullOrWhiteSpace(documento) : string.IsNullOrWhiteSpace(cliente.cta_id)))
                    return Json(new { ok = false, mensaje = "Seleccione un cliente identificado antes de consultar pendientes." });
                clienteId = cliente.cta_id;

                // ❷ OBTENER TODAS LAS FACTURAS DE LA SESIÓN
                var todasLasFacturas = FacturasPendientesActuales;

                if (todasLasFacturas == null || !todasLasFacturas.Any())
                {
                    _logger?.LogWarning("❌ No hay facturas en la sesión");
                    return Json(new { ok = true, lista = Array.Empty<FactPendienteResponseDto>(), mensaje = "No hay facturas pendientes de cobro." });
                }

                _logger?.LogInformation($"   📦 Total facturas en sesión: {todasLasFacturas.Count}");

                // ❸ FILTRAR FACTURAS DEL CLIENTE ESPECÍFICO
                var facturasDelCliente = todasLasFacturas
                    .Where(f => PerteneceAlCliente(f, cliente))
                    .ToList();

                _logger?.LogInformation($"   ✅ Facturas encontradas para cliente {clienteId}: {facturasDelCliente.Count}");

                // ❹ VALIDAR QUE SE ENCONTRARON FACTURAS
                if (!facturasDelCliente.Any())
                {
                    _logger?.LogWarning($"⚠️ No se encontraron facturas para el cliente {clienteId}");
                    return Json(new { ok = true, lista = facturasDelCliente, mensaje = "No hay facturas pendientes para este cliente." });
                }

                // ❺ LOG DETALLADO DE LAS FACTURAS ENCONTRADAS
                _logger?.LogInformation("   📋 Detalle de facturas encontradas:");
                foreach (var factura in facturasDelCliente.Take(5)) // Mostrar solo las primeras 5 en log
                {
                    _logger?.LogInformation($"      • {factura.tco_id} {factura.cm_compte} - ${factura.cv_importe:N2}");
                }

                if (facturasDelCliente.Count > 5)
                {
                    _logger?.LogInformation($"      ... y {facturasDelCliente.Count - 5} más");
                }

                _logger?.LogInformation("═══════════════════════════════════════════════════");

                return Json(new { ok = true, lista = facturasDelCliente });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error al filtrar facturas por cliente desde sesión");
                return Json(new { ok = false, mensaje = "Ocurrió un error al obtener las facturas del cliente." });
            }
        }

        /// <summary>
        /// ✅ NUEVO v3.0: Método centralizado para cargar todas las facturas pendientes.
        /// Encapsula la lógica de carga y almacenamiento en sesión.
        /// </summary>
        /// <returns>Tupla con lista de facturas y mensaje de error (si lo hay)</returns>
        private static bool PerteneceAlCliente(FactPendienteResponseDto factura, CuentaDatosResultadoDto cliente)
        {
            if (string.Equals(cliente.Origen?.Trim(), "F", StringComparison.OrdinalIgnoreCase))
                return string.IsNullOrWhiteSpace(factura.cta_id) && DocumentoCoincide(factura.co_pd_doc, cliente.cta_documento);
            return !string.IsNullOrWhiteSpace(cliente.cta_id) &&
                string.Equals(factura.cta_id?.Trim(), cliente.cta_id.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool DocumentoCoincide(string? primero, string? segundo)
        {
            var a = new string((primero ?? "").Where(char.IsDigit).ToArray());
            var b = new string((segundo ?? "").Where(char.IsDigit).ToArray());
            return a.Length > 0 && a == b;
        }

        private async Task<(List<FactPendienteResponseDto>? Facturas, string MensajeError)> CargarTodasLasFacturasPendientes()
        {
            FacturasPendientesActuales = new List<FactPendienteResponseDto>();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger?.LogInformation("🔍 Cargando todas las facturas pendientes...");

                // ❶ VALIDAR CAJA ACTUAL
                var cajaActual = CajaActual;
                if (cajaActual?.Caja == null || string.IsNullOrWhiteSpace(cajaActual.CajaId))
                {
                    _logger?.LogWarning("❌ No hay caja abierta");

                    return (
                        new List<FactPendienteResponseDto>(),
                        "No hay caja abierta. Por favor, abra una caja antes de continuar."
                    );
                }

                _logger?.LogInformation($"   Caja actual: {cajaActual.CajaId}");
                _logger?.LogInformation($"   Proceso: {cajaActual.Caja.caja_nro_proceso}");
                _logger?.LogInformation($"   Cierre: {cajaActual.Caja.caja_nro_cierre}");

                // ❷ PREPARAR REQUEST PARA BUSCAR TODAS LAS FACTURAS
                var request = new FactPendienteRequestDto
                {
                    caja_nro_cierre = cajaActual.Caja.caja_nro_cierre,
                    caja_nro_proceso = cajaActual.Caja.caja_nro_proceso,
                    cta_id = "%",           // ✅ WILDCARD: Busca TODOS los clientes
                    tdo_codigo = "",        // ✅ Todos los tipos de documento
                    cta_documento = "%",    // ✅ WILDCARD: Todos los documentos
                    tipo_carga = "T"        // ✅ Todas las cargas
                };

                // ❸ EJECUTAR CONSULTA AL SERVICIO
                var resultado = await _fdiferidoSv.ObtenerFacturasPendientes(request, TokenCookie);
                stopwatch.Stop();

                _logger?.LogInformation($"⏱️ Tiempo de consulta: {stopwatch.ElapsedMilliseconds}ms");

                // ❹ VALIDAR RESPUESTA DEL SERVICIO
                if (resultado == null || !resultado.Ok || resultado.ListaEntidad == null)
                {
                    string mensajeError =
                        resultado?.Mensaje ??
                        "No se pudieron obtener las facturas pendientes.";

                    _logger?.LogError(
                        "❌ Error al obtener facturas: {Mensaje}",
                        mensajeError
                    );

                    return (
                        new List<FactPendienteResponseDto>(),
                        mensajeError
                    );
                }

                // ❺ RESGUARDAR EN SESIÓN (para operaciones posteriores)
                var facturas = resultado.ListaEntidad ?? new List<FactPendienteResponseDto>();
                FacturasPendientesActuales = facturas;

                _logger?.LogInformation($"✅ Se obtuvieron {facturas.Count} facturas pendientes");
                _logger?.LogInformation($"   Guardadas en sesión del servidor");

                // ❻ LOG DETALLADO DE CLIENTES ÚNICOS (para debugging)
                if (facturas.Count > 0)
                {
                    var clientesUnicos = facturas
                        .Select(f => new { f.cta_id, f.co_pd_nombre })
                        .Distinct()
                        .Take(10)  // Limitar a 10 para no saturar logs
                        .ToList();

                    _logger?.LogInformation($"   Clientes con deuda (primeros 10):");
                    foreach (var cliente in clientesUnicos)
                    {
                        var cantFacturas = facturas.Count(f => f.cta_id == cliente.cta_id);
                        _logger?.LogInformation($"      • {cliente.co_pd_nombre} ({cliente.cta_id}): {cantFacturas} factura(s)");
                    }
                }

                return (facturas, string.Empty);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "❌ Excepción al cargar facturas pendientes");
                _logger?.LogError($"   Tiempo hasta el error: {stopwatch.ElapsedMilliseconds}ms");
                return (new List<FactPendienteResponseDto>(), "Ocurrió un error inesperado al cargar las facturas.");
            }
        }


        public IActionResult Inicializa()
        {
            // Esta acción redirige a la vista principal del módulo.
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Se resguardan las facturas seleccionadas 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResguardarFacturasPendientesSeleccionadas([FromBody] FactsPendienteDto req)
        {
            List<FactPendienteResponseDto> facturasSeleccionadas = new();
            try
            {
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📥 RESGUARDAR FACTURAS PENDIENTES SELECCIONADAS v2.0");
                _logger?.LogInformation($"   Usuario: {UserName}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                if (!VerificarAutenticacion(out _))
                {
                    _logger?.LogWarning("❌ Sesión expirada al intentar resguardar facturas");
                    return Json(new { ok = false, mensaje = "Sesión expirada. Por favor, inicie sesión de nuevo." });
                }

                _logger?.LogInformation($"   Request recibido: {(req == null ? "NULL" : $"{req.Facturas.Count} facturas")}");

                if (req == null || req.Facturas == null)
                {
                    _logger?.LogWarning("❌ Se recibió una solicitud nula para resguardar facturas.");
                    return Json(new { ok = false, mensaje = "La solicitud no puede ser nula. Verifique el formato de los datos enviados." });
                }

                if (req.Facturas.Count == 0)
                {
                    _logger?.LogWarning("⚠️ Se recibió una lista vacía de facturas");
                    return Json(new { ok = false, mensaje = "No se recibieron facturas para resguardar." });
                }

                _logger?.LogInformation($"   📦 Procesando {req.Facturas.Count} facturas:");
                for (int i = 0; i < req.Facturas.Count; i++)
                {
                    var factura = req.Facturas[i];
                    _logger?.LogInformation($"      [{i + 1}] {factura.tco_id} {factura.cm_compte} - ${factura.cv_importe:N2} - {factura.co_pd_nombre}");

                    if (string.IsNullOrEmpty(factura.tco_id) || string.IsNullOrEmpty(factura.cm_compte))
                    {
                        _logger?.LogWarning($"      ⚠️ Factura [{i + 1}] tiene datos críticos vacíos");
                    }
                    facturasSeleccionadas.Add(factura);
                }

                // ✅ IMPORTANTE: Estas son las facturas SELECCIONADAS para cobrar
                // NO reemplazamos FacturasPendientesActuales (que contiene TODAS las facturas)
                // sino que las guardamos en una variable de sesión DIFERENTE
                FacturasSeleccionadasParaCobro = facturasSeleccionadas;

                _logger?.LogInformation($"   ✅ Se han resguardado {req.Facturas.Count} facturas SELECCIONADAS en la sesión del servidor.");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                return Json(new { ok = true, mensaje = "Facturas seleccionadas guardadas correctamente." });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error al resguardar las facturas pendientes seleccionadas.");
                return Json(new { ok = false, mensaje = "Ocurrió un error inesperado al guardar las facturas." });
            }
        }

        /// <summary>
        /// Obtiene las facturas pendientes que fueron previamente resguardadas en la sesión.
        /// </summary>
        [HttpPost]
        public JsonResult ObtenerFacturasPendientesSesion()
        {
            try
            {
                if (!VerificarAutenticacion(out _))
                {
                    return Json(new { ok = false, mensaje = "Sesión expirada." });
                }

                // ✅ CORRECCIÓN: Ahora devolvemos las facturas SELECCIONADAS, no todas
                var facturasEnSesion = FacturasSeleccionadasParaCobro;

                if (facturasEnSesion == null || !facturasEnSesion.Any())
                {
                    _logger?.LogWarning("Se intentó obtener facturas de la sesión, pero no había ninguna.");
                    return Json(new { ok = false, mensaje = "No se encontraron facturas pendientes en la sesión." });
                }

                _logger?.LogInformation("Se recuperaron {Count} facturas pendientes desde la sesión.", facturasEnSesion.Count);

                //// Limpiamos la sesión después de recuperarlas
                //FacturasSeleccionadasParaCobro = null;

                return Json(new { ok = true, lista = facturasEnSesion });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener las facturas pendientes desde la sesión.");
                return Json(new { ok = false, mensaje = "Ocurrió un error inesperado al obtener las facturas." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ObtenerFacturasPendientes()
        {
            try
            {
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🔄 RECARGAR FACTURAS DIFERIDAS PENDIENTES");
                _logger?.LogInformation($"   Usuario: {UserName}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                if (!VerificarAutenticacion(out _))
                {
                    _logger?.LogWarning(
                        "❌ Sesión expirada al recargar facturas diferidas."
                    );

                    return Json(new
                    {
                        ok = false,
                        mensaje = "Sesión expirada. Por favor, inicie sesión nuevamente."
                    });
                }

                // Esta función consulta todas las pendientes del día y,
                // además, actualiza FacturasPendientesActuales en sesión.
                var resultado = await CargarTodasLasFacturasPendientes();

                if (!string.IsNullOrWhiteSpace(resultado.MensajeError))
                {
                    _logger?.LogWarning(
                        "⚠️ No se pudieron recargar las facturas pendientes: {Mensaje}",
                        resultado.MensajeError
                    );

                    return Json(new
                    {
                        ok = false,
                        mensaje = resultado.MensajeError
                    });
                }

                var facturasActualizadas =
                    resultado.Facturas ?? new List<FactPendienteResponseDto>();

                // La selección anterior ya fue cobrada o abandonada.
                // No debe contaminar una nueva cobranza.
                FacturasSeleccionadasParaCobro =
                    new List<FactPendienteResponseDto>();

                _logger?.LogInformation(
                    "✅ Facturas diferidas actualizadas. Pendientes actuales: {Cantidad}",
                    facturasActualizadas.Count
                );

                _logger?.LogInformation(
                    "✅ Selección temporal FacturasSeleccionadasParaCobro limpiada."
                );

                _logger?.LogInformation("═══════════════════════════════════════════════════");

                return Json(new
                {
                    ok = true,
                    lista = facturasActualizadas,
                    cantidad = facturasActualizadas.Count,
                    tienePendientesCliente = ClienteActual == null ? (bool?)null : facturasActualizadas.Any(f => PerteneceAlCliente(f, ClienteActual)),
                    mensaje = facturasActualizadas.Count > 0
                        ? "Facturas pendientes actualizadas correctamente."
                        : "No hay facturas pendientes de cobro."
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "❌ Error al recargar las facturas diferidas pendientes."
                );

                return Json(new
                {
                    ok = false,
                    mensaje = "Ocurrió un error inesperado al actualizar las facturas pendientes."
                });
            }
        }
    }
}
