using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Seguridad;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Options;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class InfoProdController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<InfoProdController> _logger;
        private readonly IProductoServicio _productoServicio;
        private readonly ICuentaServicio _ctaSv;
        private readonly IRubroServicio _rubSv;

        public InfoProdController(IOptions<AppSettings> options, IOptions<MenuSettings> options1,
            IHttpContextAccessor context, IProductoServicio productoServicio, ILogger<InfoProdController> logger,
            ICuentaServicio cta, IRubroServicio rubro) : base(options, options1, context)
        {
            _menuSettings = options1.Value;
            _productoServicio = productoServicio;
            _logger = logger;
            _ctaSv = cta;
            _rubSv = rubro;
        }

        /// <summary>
        /// CODIGO DEL MODULO "INFO"
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(bool actualizar = false)
        {
           
            try
            {
                try
                {
                    if (!EstaAutenticado.Item1)
                    {
                        return RedirectToAction("Login", "Token", new { area = "Seguridad" });
                    }
                    if (ProveedoresLista.Count == 0 || actualizar)
                    {
                        ObtenerProveedores();
                    }

                    if (RubroLista.Count == 0 || actualizar)
                    {
                        ObtenerRubros();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en la carga de datos periféricos.");
                    TempData["error"] = "Hubo algún error al intentar cargar la vista de autenticación. Si el problema persiste, avise al administardor.";
                    var lv = new List<AdministracionLoginDto>();
                    ViewBag.Admid = HelperMvc<AdministracionLoginDto>.ListaGenerica(lv);
                    var login = new LoginDto { Fecha = DateTime.Now };
                }

                var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals("info", StringComparison.OrdinalIgnoreCase));
                if (modulo == null)
                {
                    throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
                }
                return View(modulo);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("index", "home", new { area = string.Empty });
            }
        }

        private void ObtenerRubros()
        {
            RubroLista = _rubSv.ObtenerListaRubros(TokenCookie);
        }

        private void ObtenerProveedores()
        {
            //se guardan los proveedores en session. Para ser utilizados posteriormente

            ProveedoresLista = _ctaSv.ObtenerListaProveedores(TokenCookie);
        }


        [HttpGet]
        public IActionResult Producto()
        {

            return View();
        }

        [HttpGet]
        public IActionResult Depo()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Box()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Ul()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> InfoProductoStkD(string id)
        {
            try
            {
                var regs =await _productoServicio.InfoProductoStkD(id, AdministracionId,TokenCookie);

                if(regs==null || regs.Count == 0)
                {
                    return Json(new {error = false,list = regs, cantRegs = 0});
                }
                else
                {
                    return Json(new {error = false,list = regs, cantRegs = regs.Count});
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return Json(new {error = true,msg= ex.Message});
                
            }
        }

        [HttpPost]
        public async Task<JsonResult> InfoProductoStkBoxes(string id,string depo)
        {
            try
            {
                var regs = await _productoServicio.InfoProductoStkBoxes(id, AdministracionId,depo, TokenCookie);

                if (regs == null || regs.Count == 0)
                {
                    return Json(new { error = false, list = regs, cantRegs = 0 });
                }
                else
                {
                    return Json(new { error = false, list = regs, cantRegs = regs.Count });
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return Json(new { error = true, msg = ex.Message });

            }
        }

        [HttpPost]
        public async Task<JsonResult> InfoProductoStkA(string id)
        {
            try
            {
                var regs = await _productoServicio.InfoProductoStkA(id, AdministracionId, TokenCookie);

                if (regs == null || regs.Count == 0)
                {
                    return Json(new { error = false, list = regs, cantRegs = 0 });
                }
                else
                {
                    return Json(new { error = false, list = regs, cantRegs = regs.Count });
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return Json(new { error = true, msg = ex.Message });

            }
        }

        [HttpPost]
        public async Task<JsonResult> InfoProductoMovStk(string id,string depo,string tmov, DateTime desde,DateTime hasta)
        {
            try
            {
                var regs = await _productoServicio.InfoProductoMovStk(id, AdministracionId,depo,tmov,desde,hasta, TokenCookie);

                if (regs == null || regs.Count == 0)
                {
                    return Json(new { error = false, list = regs, cantRegs = 0 });
                }
                else
                {
                    return Json(new { error = false, list = regs, cantRegs = regs.Count });
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return Json(new { error = true, msg = ex.Message });

            }
        }

        [HttpPost]
        public async Task<JsonResult> InfoProductoLP(string id)
        {
            try
            {
                var regs = await _productoServicio.InfoProductoLP(id, TokenCookie);

                if (regs == null || regs.Count == 0)
                {
                    return Json(new { error = false, list = regs, cantRegs = 0 });
                }
                else
                {
                    return Json(new { error = false, list = regs, cantRegs = regs.Count });
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return Json(new { error = true, msg = ex.Message });

            }
        }
    }
}
