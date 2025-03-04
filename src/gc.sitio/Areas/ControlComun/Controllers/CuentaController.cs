using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.EntidadesComunes.ControlComun.CuentaComercial;
using gc.sitio.Controllers;
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

        public CuentaController(ILogger<CuentaController> logger, ICuentaServicio cuentaServicio, IOptions<AppSettings> options1, IHttpContextAccessor context) : base(options1, context)
        {
            _logger = logger;
            _cuentaServicio = cuentaServicio;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TestCC()
        {

            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
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


        [HttpPost]
        public async Task<JsonResult> BusquedaCuenta(string cuenta, char tipo, bool esAutoComp)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                List<CuentaDto> cuentas = await _cuentaServicio.ObtenerListaCuentaComercial(cuenta, tipo, TokenCookie);
                if (esAutoComp)
                {
                    foreach (var cta in cuentas)
                    {
                        cta.Cta_Denominacion = cta.Cta_Denominacion.QuitarEspaciosBlancosExtra();
                    }
                    var lista = cuentas.Select(x => new { Id = x.Cta_Id, Descripcion = x.Cta_Denominacion, ProvId = x.Prov_Id });
                    return Json(lista );
                }
                else
                {
                    if (cuentas.Count == 1)
                    {
                        return Json(new { error = false, warn = false, unico = true, lista = cuentas });
                    }
                    else
                    {
                        foreach (var cta in cuentas)
                        {
                            cta.Cta_Denominacion = cta.Cta_Denominacion.QuitarEspaciosBlancosExtra();
                        }

                        return Json(new { error = false, warn = false, lista = cuentas });
                    }
                }

            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Búsqueda Cuenta Comercial";
                _logger.LogError(ex, msg);

                return Json(new { error = true, msg });
            }
        }
    }
}
