using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class PDiferidoController : ControladorBaseCaja
    {

        private readonly IFactDiferidaServicio _fdiferidoSv;
        public PDiferidoController(IOptions<AppSettings> options, IHttpContextAccessor contexto,
            ILogger<PDiferidoController> logger, IFactDiferidaServicio fdiferidoSv) : base(options, contexto, logger)
        {
            _fdiferidoSv = fdiferidoSv;
        }

        public IActionResult Index()
        {
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

        public IActionResult Inicializa()
        {
            // Esta acción redirige a la vista principal del módulo.
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Este método maneja la solicitud POST para obtener las facturas pendientes de cobranza diferida para el cliente y caja actuales.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ObtenerFacturasPendientes()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // ❶ VALIDAR AUTENTICACIÓN
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "Sesión expirada" });

                // ❷ Cliente Actual
                var clienteActual = ClienteActual;
                if (clienteActual == null)
                {
                    stopwatch.Stop();
                    _logger?.LogInformation($"⏱️ Tiempo antes del bloqueo: {stopwatch.ElapsedMilliseconds}ms");
                    _logger?.LogWarning("❌ No hay cliente seleccionado");
                    return Json(new { ok = false, mensaje = "Debe seleccionar un cliente primero" });
                }

                // ❸ Caja Actual
                var cajaActual = CajaActual;
                if (cajaActual == null)
                {
                    stopwatch.Stop();
                    _logger?.LogInformation($"⏱️ Tiempo antes del bloqueo: {stopwatch.ElapsedMilliseconds}ms");
                    _logger?.LogWarning("❌ No hay caja abierta");
                    return Json(new { ok = false, mensaje = "No hay caja abierta" });
                }


                var request = new FactPendienteRequestDto
                {
                    caja_nro_cierre = cajaActual.Caja.caja_nro_cierre,
                    caja_nro_proceso = cajaActual.Caja.caja_nro_proceso,
                    cta_id = clienteActual.cta_id,
                    tdoc_id = clienteActual.tdoc_id,
                    cta_documento = clienteActual.cta_documento,
                    carga = "T"
                };

                var resultado = await _fdiferidoSv.ObtenerFacturasPendientes(request, TokenCookie);
                stopwatch.Stop();
                _logger?.LogInformation($"⏱️ Tiempo de ejecución: {stopwatch.ElapsedMilliseconds}ms");

                if (!resultado.Ok)
                {
                    _logger?.LogError("❌ Error al obtener facturas pendientes: {Mensaje}", resultado.Mensaje);
                    return Json(new { ok = false, mensaje = resultado.Mensaje });
                }

                return Json(new { ok = true, lista = resultado.ListaEntidad });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Excepción al obtener facturas pendientes");
                return Json(new { ok = false, mensaje = "Ocurrió un error al obtener las facturas pendientes" });
            }
            finally
            {
                stopwatch.Stop();
                _logger?.LogInformation("⏱️ ObtenerFacturasPendientes ejecutado en {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
