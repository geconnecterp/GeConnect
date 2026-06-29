using gc.caja.Controllers;
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
        public JsonResult ObtenerFacturasClienteDesdesesion([FromBody] string clienteId)
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

                // ❶ VALIDAR QUE EL clienteId NO ESTÉ VACÍO
                if (string.IsNullOrWhiteSpace(clienteId))
                {
                    _logger?.LogWarning("❌ clienteId vacío o nulo");
                    return Json(new { ok = false, mensaje = "El ID del cliente es requerido." });
                }

                // ❷ OBTENER TODAS LAS FACTURAS DE LA SESIÓN
                var todasLasFacturas = FacturasPendientesActuales;

                if (todasLasFacturas == null || !todasLasFacturas.Any())
                {
                    _logger?.LogWarning("❌ No hay facturas en la sesión");
                    return Json(new { ok = false, mensaje = "No hay facturas cargadas en la sesión. Por favor, recargue la página." });
                }

                _logger?.LogInformation($"   📦 Total facturas en sesión: {todasLasFacturas.Count}");

                // ❸ FILTRAR FACTURAS DEL CLIENTE ESPECÍFICO
                var facturasDelCliente = todasLasFacturas
                    .Where(f => f.cta_id != null && f.cta_id.Equals(clienteId, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                _logger?.LogInformation($"   ✅ Facturas encontradas para cliente {clienteId}: {facturasDelCliente.Count}");

                // ❹ VALIDAR QUE SE ENCONTRARON FACTURAS
                if (!facturasDelCliente.Any())
                {
                    _logger?.LogWarning($"⚠️ No se encontraron facturas para el cliente {clienteId}");
                    return Json(new { ok = false, mensaje = $"No se encontraron facturas pendientes para el cliente {clienteId}." });
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
        private async Task<(List<FactPendienteResponseDto>? Facturas, string MensajeError)> CargarTodasLasFacturasPendientes()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger?.LogInformation("🔍 Cargando todas las facturas pendientes...");

                // ❶ VALIDAR CAJA ACTUAL
                var cajaActual = CajaActual;
                if (cajaActual == null || string.IsNullOrEmpty(cajaActual.CajaId))
                {
                    _logger?.LogWarning("❌ No hay caja abierta");
                    return (new List<FactPendienteResponseDto>(), "No hay caja abierta. Por favor, abra una caja antes de continuar.");
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
                if (!resultado.Ok || resultado == null || resultado.ListaEntidad == null)
                {
                    string mensajeError = resultado?.Mensaje ?? "No se pudieron obtener las facturas pendientes.";
                    _logger?.LogError($"❌ Error al obtener facturas: {mensajeError}");
                    return (new List<FactPendienteResponseDto>(), mensajeError);
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

        ///// <summary>
        ///// Este método maneja la solicitud POST para obtener las facturas pendientes de cobranza diferida para el cliente y caja actuales.
        ///// </summary>
        ///// <returns></returns>

        //private async Task<(bool,string)> VerFacturasPendientes()
        //{
        //    var stopwatch = Stopwatch.StartNew();

        //    try
        //    {
        //        // ❸ Caja Actual
        //        var cajaActual = CajaActual;
        //        if (cajaActual == null)
        //        {
        //            stopwatch.Stop();
        //            _logger?.LogInformation($"⏱️ Tiempo antes del bloqueo: {stopwatch.ElapsedMilliseconds}ms");
        //            _logger?.LogWarning("❌ No hay caja abierta");
        //            return (false, "No hay caja abierta" );
        //        }


        //        var request = new FactPendienteRequestDto
        //        {
        //            caja_nro_cierre = cajaActual.Caja.caja_nro_cierre,
        //            caja_nro_proceso = cajaActual.Caja.caja_nro_proceso,
        //            cta_id = "%",
        //            tdo_codigo = "",
        //            cta_documento = "%",
        //            tipo_carga = "T"
        //        };

        //        var resultado = await _fdiferidoSv.ObtenerFacturasPendientes(request, TokenCookie);
        //        stopwatch.Stop();
        //        _logger?.LogInformation($"⏱️ Tiempo de ejecución: {stopwatch.ElapsedMilliseconds}ms");

        //        if (!resultado.Ok || resultado==null || resultado.ListaEntidad==null)
        //        {
        //            _logger?.LogError("❌ Error al obtener facturas pendientes: {Mensaje}", resultado.Mensaje ?? "Hubo problemas al intentar recuperar las facturas pendientes");
        //            return(false, resultado.Mensaje ?? "Hubo problemas al intentar recuperar las facturas pendientes" );
        //        }
        //        //se resguardan las facturas pendientes existan o no, si ha termiando OK
        //        FacturasPendientesActuales = resultado.ListaEntidad;
        //        return (true, "");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger?.LogError(ex, "❌ Excepción al obtener facturas pendientes");
        //        return (false, "Ocurrió un error al obtener las facturas pendientes");
        //    }
        //    finally
        //    {
        //        stopwatch.Stop();
        //        _logger?.LogInformation("⏱️ ObtenerFacturasPendientes ejecutado en {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
        //    }
        //}

        ///// <summary>
        ///// Este método maneja la solicitud POST para obtener las facturas pendientes de cobranza diferida para el cliente y caja actuales.
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<JsonResult> ObtenerFacturasPendientes()
        //{
        //    var stopwatch = Stopwatch.StartNew();

        //    try
        //    {
        //        // ❶ VALIDAR AUTENTICACIÓN
        //        if (!VerificarAutenticacion(out IActionResult redirectResult))
        //            return Json(new { ok = false, mensaje = "Sesión expirada" });

        //        // ❷ Cliente Actual
        //        var clienteActual = ClienteActual;
        //        if (clienteActual == null)
        //        {
        //            stopwatch.Stop();
        //            _logger?.LogInformation($"⏱️ Tiempo antes del bloqueo: {stopwatch.ElapsedMilliseconds}ms");
        //            _logger?.LogWarning("❌ No hay cliente seleccionado");
        //            return Json(new { ok = false, mensaje = "Debe seleccionar un cliente primero" });
        //        }

        //        // ❸ Caja Actual
        //        var cajaActual = CajaActual;
        //        if (cajaActual == null)
        //        {
        //            stopwatch.Stop();
        //            _logger?.LogInformation($"⏱️ Tiempo antes del bloqueo: {stopwatch.ElapsedMilliseconds}ms");
        //            _logger?.LogWarning("❌ No hay caja abierta");
        //            return Json(new { ok = false, mensaje = "No hay caja abierta" });
        //        }


        //        var request = new FactPendienteRequestDto
        //        {
        //            caja_nro_cierre = cajaActual.Caja.caja_nro_cierre,
        //            caja_nro_proceso = cajaActual.Caja.caja_nro_proceso,
        //            cta_id = clienteActual.cta_id,
        //            tdo_codigo = clienteActual.tdoc_id,
        //            cta_documento = clienteActual.cta_documento,
        //            tipo_carga = ""
        //        };

        //        var resultado = await _fdiferidoSv.ObtenerFacturasPendientes(request, TokenCookie);
        //        stopwatch.Stop();
        //        _logger?.LogInformation($"⏱️ Tiempo de ejecución: {stopwatch.ElapsedMilliseconds}ms");

        //        if (!resultado.Ok)
        //        {
        //            _logger?.LogError("❌ Error al obtener facturas pendientes: {Mensaje}", resultado.Mensaje);
        //            return Json(new { ok = false, mensaje = resultado.Mensaje });
        //        }

        //        return Json(new { ok = true, lista = resultado.ListaEntidad });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger?.LogError(ex, "❌ Excepción al obtener facturas pendientes");
        //        return Json(new { ok = false, mensaje = "Ocurrió un error al obtener las facturas pendientes" });
        //    }
        //    finally
        //    {
        //        stopwatch.Stop();
        //        _logger?.LogInformation("⏱️ ObtenerFacturasPendientes ejecutado en {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
        //    }
        //}
    }
}
