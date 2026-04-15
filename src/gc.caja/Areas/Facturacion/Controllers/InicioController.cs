using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class InicioController : ControladorBaseCaja
    {
        private readonly ICajaInitServicio _cajaInitSv;
        //private readonly ILogger<InicioController> _logger;

        public InicioController(
            IOptions<AppSettings> options, 
            ICajaInitServicio cajaInitSv,
            IHttpContextAccessor httpContext,
            ILogger<InicioController> logger) : base(options, httpContext, logger)
        {
            _cajaInitSv = cajaInitSv;
           
        }

        /// <summary>
        /// Presenta la vista principal del módulo de facturación
        /// </summary>
        [HttpGet]
        public IActionResult index()
        {
            try
            {
                var caja = CajaActual;
                
                if (caja == null || string.IsNullOrEmpty(caja.CajaId))
                {
                    _logger?.LogWarning("Acceso a Facturación sin caja configurada. Usuario: {Usuario}", UserName);
                    TempData["Error"] = "No se ha configurado una caja para esta estación.";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

                ViewBag.Usuario = UserName;
                ViewBag.CajaId = caja.Caja.caja_nombre;
                ViewBag.CajaNombre = caja.Caja?.caja_nombre ?? "N/A";

                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cargar vista de Facturación. Usuario: {Usuario}", UserName);
                TempData["Error"] = "Error al cargar el módulo de facturación.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        /// <summary>
        /// Valida que los datos de la caja sean correctos antes de iniciar facturación
        /// </summary>
        [HttpPost]
        public JsonResult ValidacionInicial()
        {
            try
            {
                var caja = CajaActual;

                // ✅ Validar que existe la caja
                if (caja == null)
                {
                    _logger?.LogWarning("ValidacionInicial: CajaActual es null. Usuario: {Usuario}", UserName);
                    return Json(new
                    {
                        success = false,
                        message = "No se ha configurado una caja para esta estación.",
                        detalle = "CajaActual es null"
                    });
                }

                if (string.IsNullOrEmpty(caja.CajaId))
                {
                    _logger?.LogWarning("ValidacionInicial: CajaId vacío. Usuario: {Usuario}", UserName);
                    return Json(new
                    {
                        success = false,
                        message = "La caja no tiene un identificador válido.",
                        detalle = "CajaId vacío"
                    });
                }

                // ✅ Verificar que el objeto Caja existe
                if (caja.Caja == null)
                {
                    _logger?.LogWarning("ValidacionInicial: caja.Caja es null. CajaId: {CajaId}, Usuario: {Usuario}", 
                        caja.CajaId, UserName);
                    return Json(new
                    {
                        success = false,
                        message = "Los datos de la caja no están disponibles. Por favor, cierre sesión y vuelva a abrir la caja.",
                        detalle = "caja.Caja es null"
                    });
                }

                // ✅ CORRECCIÓN: Desestructurar la tupla correctamente
                var (esValido, mensajeValidacion) = _cajaInitSv.ValidarDatosIniciales(caja);

                if (!esValido)
                {
                    _logger?.LogWarning("ValidacionInicial: Validación fallida. CajaId: {CajaId}, Razón: {Razon}", 
                        caja.CajaId, mensajeValidacion);
                    
                    return Json(new
                    {
                        success = false,
                        message = mensajeValidacion,
                        detalle = "Validación de integridad fallida",
                        caja_id = caja.CajaId
                    });
                }

                // ✅ Validación exitosa
                _logger?.LogInformation("ValidacionInicial: Validación exitosa. CajaId: {CajaId}, Usuario: {Usuario}", 
                    caja.CajaId, UserName);
                
                return Json(new
                {
                    success = true,
                    message = mensajeValidacion,
                    caja_id = caja.CajaId,
                    usuario = UserName,
                    caja_nombre = caja.Caja.caja_nombre
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error en ValidacionInicial. Usuario: {Usuario}, CajaId: {CajaId}", 
                    UserName, CajaActual?.CajaId ?? "NULL");

                return Json(new
                {
                    success = false,
                    message = "Error interno al validar los datos de la caja. Por favor, contacte al administrador.",
                    detalle = ex.Message
                });
            }
        }
    }
}
