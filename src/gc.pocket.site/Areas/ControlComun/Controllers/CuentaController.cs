using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.EntidadesComunes.ControlComun.CuentaComercial;
using gc.pocket.site.Areas.Gestion.Controllers;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.pocket.site.Areas.ControlComun.Controllers
{
    [Area("ControlComun")]
    public class CuentaController : ControladorBase
    {
        private readonly ILogger<CuentaController> _logger;
        private readonly ICuentaServicio _cuentaServicio;

        public CuentaController(ILogger<CuentaController> logger, ICuentaServicio cuentaServicio, IOptions<AppSettings> options1, IHttpContextAccessor context) : base(options1, context,logger)
        { 
            _logger = logger;
            _cuentaServicio = cuentaServicio;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> BusquedaCuentaComercial(BusquedaCuentaComercial search)
        {
            try
            {
                List<CuentaDto> cuentasComerciales = await _cuentaServicio.ObtenerListaCuentaComercial(search.Texto, search.TipoBusqueda, TokenCookie);
                return Json(new { error = false, lista = cuentasComerciales });
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Búsqueda Cuenta Comercial";
                _logger.LogError(ex, "Error en la invocación de la API - Búsqueda Cuenta Comercial");

                return Json(new { error = true, msg });
            }
        }
    }
}
