using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Seguridad;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.pocket.site.Areas.Gestion.Controllers
{
    [Area("Gestion")]
    public class ProductoController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<ProductoController> _logger;
        private readonly ICuentaServicio _ctaSv;
        private readonly IRubroServicio _rubSv;

        public ProductoController(ILogger<ProductoController> logger, IOptions<MenuSettings> options, IOptions<AppSettings> options1,
            ICuentaServicio cuentaServicio, IHttpContextAccessor context, IRubroServicio rubSv) : base(options1, options, context)
        {
            _logger = logger;
            _menuSettings = options.Value;
            _ctaSv = cuentaServicio;
            _rubSv = rubSv;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Busqueda(bool actualizar = false)
        {
            try
            {
                if (ProveedoresLista.Count == 0 || actualizar)
                {
                    ObtenerProveedores();
                }

                if (RubroLista.Count == 0 || actualizar)
                {
                    ObtenerRubros();
                }

                return View(new List<ProductoDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Login");
                TempData["error"] = "Hubo algún error al intentar cargar la vista de autenticación. Si el problema persiste, avise al administardor.";
                var lv = new List<AdministracionLoginDto>();
                ViewBag.Admid = HelperMvc<AdministracionLoginDto>.ListaGenerica(lv);
                var login = new LoginDto { Fecha = DateTime.Now };
                return View(new List<ProductoDto>());
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
    }
}
