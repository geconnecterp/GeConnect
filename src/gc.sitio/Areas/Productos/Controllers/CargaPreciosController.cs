using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Controllers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class CargaPreciosController : ControladorBase
    {
        private readonly AppSettings _appSettings;
        private readonly IAdministracionServicio _administracionServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IRubroServicio _rubroServicio;
        private readonly IProductoServicio _productoServicio;

        public CargaPreciosController(
            ICuentaServicio cuentaServicio,
            IRubroServicio rubroServicio,
            IProductoServicio productoServicio,
            IAdministracionServicio administracionServicio,
            ILogger<CompraController> logger,
            IOptions<AppSettings> options,
            IHttpContextAccessor context) : base(options, context)
        {
            _administracionServicio = administracionServicio;
            _cuentaServicio = cuentaServicio;
            _rubroServicio = rubroServicio;
            _productoServicio = productoServicio;
            _appSettings = options.Value;
        }

        public IActionResult Index()
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                var listR01 = new List<ComboGenDto>();
                ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

                var listR02 = new List<ComboGenDto>();
                ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

                var listR03 = new List<ComboGenDto>();
                ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);
                ViewBag.Rel03 = HelperMvc<ComboGenDto>.ListaGenerica(listR03);

                string titulo = "Productos - Carga de Precios";
                ViewData["Titulo"] = titulo;
                CargarDatosIniciales(true);

                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cargar la vista de BSS");
                TempData["error"] = "Hubo un problema al cargar la vista del BSS. Si el problema persiste, contacte al administrador.";
                return View();
            }
        }

        //Invocar cuando se haya seleccionado solo un proveedor desde el filtro base.
        [HttpPost]
        public JsonResult BuscarFamiliaDesdeProveedorSeleccionado(string ctaId)
        {
            try
            {
                CargarProveedoresFamiliaLista(ctaId, _cuentaServicio);
                var familias = ProveedorFamiliaLista;
                var combo = familias.Select(x => new ComboGenDto { Id = x.pg_id, Descripcion = x.pg_desc})
                    .ToList();
                return Json(new { error = false, warn = false, lista= combo });
            }
            catch (Exception)
            {
                return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar obtener los datos de la familia de productos del proveedor: {ctaId}" });
            }

        }
       
        protected void CargarProveedoresFamiliaLista(string ctaId, ICuentaServicio _cuentaServicio, string? fam = null)
        {
            var adms = _cuentaServicio.ObtenerListaProveedoresFamilia(ctaId, TokenCookie);
            ProveedorFamiliaLista = adms;
        }

        private SelectList ComboProveedores()
        {
            var adms = _cuentaServicio.ObtenerListaProveedores("BI", TokenCookie);
            var lista = adms.Select(x => new ComboGenDto { Id = x.Cta_Id, Descripcion = x.Cta_Denominacion });
            return HelperMvc<ComboGenDto>.ListaGenerica(lista);
        }

        private SelectList ComboRubros()
        {
            var adms = _rubroServicio.ObtenerListaRubros("", TokenCookie);
            var lista = adms.Select(x => new ComboGenDto { Id = x.Rub_Id, Descripcion = x.Rub_Desc });
            return HelperMvc<ComboGenDto>.ListaGenerica(lista);
        }

        private SelectList ComboSucursales()
        {
            var adms = _administracionServicio.GetAdministracionLogin();
            var lista = adms.Select(x => new ComboGenDto { Id = x.Id, Descripcion = x.Descripcion });
            return HelperMvc<ComboGenDto>.ListaGenerica(lista);
        }
        private void CargarDatosIniciales(bool actualizar)
        {
            if (ProveedoresLista.Count == 0 || actualizar)
            {
                ObtenerProveedores(_cuentaServicio, "BI");
            }

            if (RubroLista.Count == 0 || actualizar)
            {
                ObtenerRubros(_rubroServicio);
            }
        }
    }
}
