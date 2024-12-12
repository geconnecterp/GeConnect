using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.ABMs.Controllers
{
    [Area("ABMs")]
    public class AbmProductoController : ProductoControladorBase
    {
        private readonly AppSettings _settings;
        private readonly IABMProductoServicio _abmProdServ;
        private readonly ICuentaServicio _ctaSv;
        private readonly IRubroServicio _rubSv;
		private readonly ILogger<AbmProductoController> _logger;

		public AbmProductoController(IOptions<AppSettings> options, IHttpContextAccessor accessor, IABMProductoServicio productoServicio,
             ICuentaServicio cta, IRubroServicio rubro, ILogger<AbmProductoController> logger) : base(options, accessor, logger)
        {
            _settings = options.Value;
            _abmProdServ = productoServicio;
            _ctaSv = cta;
            _rubSv = rubro;
        }

        [HttpGet]
        public async Task<IActionResult> Index( bool actualizar = false)
        {
            List<ProductoListaDto> lista;
            MetadataGrid metadata;
            GridCore<ProductoListaDto> grillaDatos;
            
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            
            if (ProveedoresLista.Count == 0 || actualizar)
            {
                ObtenerProveedores(_ctaSv);
            }

            if (RubroLista.Count == 0 || actualizar)
            {
                ObtenerRubros(_rubSv);
            }
            

            string volver = Url.Action("index", "home", new { area = "" });
            ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

            var listR01 = new List<ComboGenDto>();
            ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
            var listR02 = new List<ComboGenDto>();
            ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string sort = "p_desc", string sortDir = "asc", int pag = 1, bool actualizar = false)
        {
            List<ProductoListaDto> lista;
            MetadataGrid metadata;
            GridCore<ProductoListaDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (PaginaProd == pag && !buscaNew)
                {
                    //es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
                    lista = ProductosBuscados.ToList();
                    lista = OrdenarEntidad(lista, sortDir, sort);
                    ProductosBuscados = lista;
                }
                else
                {
                    PaginaProd = pag;
                    //traemos datos desde la base
                    query.Sort = sort;
                    query.SortDir = sortDir;
                    query.Registros = _settings.NroRegistrosPagina;
                    query.Pagina = pag;

                    var res = await _abmProdServ.BuscarProducto(query, TokenCookie);
                    lista = res.Item1 ?? [];
                    MetadataProd = res.Item2 ?? null;
                    ProductosBuscados = lista;
                }
                metadata = MetadataProd;

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrilla(ProductosBuscados, sort, _settings.NroRegistrosPagina, pag, MetadataProd.TotalCount, MetadataProd.TotalPages, sortDir);

                string volver = Url.Action("index", "home", new { area = "" });
                ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridAbmProds", grillaDatos);
            }
            catch (Exception ex) {

                string msg = "Error en la invocación de la API - Busqueda Producto";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda Producto");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public JsonResult ObtenerDatosPaginacion()
        {
            try
            {
                return Json(new {error = false, Metadata = MetadataProd });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, msg = "No se pudo obtener la información de paginación. Verifica" });
            }
        }

        //[HttpPost]
        //public JsonResult BuscarProvs(string prefix)
        //{
        //    //var nombres = await _provSv.BuscarAsync(new QueryFilters { Search = prefix }, TokenCookie);
        //    //var lista = nombres.Item1.Select(c => new EmpleadoVM { Nombre = c.NombreCompleto, Id = c.Id, Cuil = c.CUIT });
        //    var rub = ProveedoresLista.Where(x => x.Cta_Lista.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
        //    var rubros = rub.Select(x => new ComboGenDto { Id = x.Cta_Id, Descripcion = x.Cta_Lista });
        //    return Json(rubros);
        //}

        //[HttpPost]
        //public JsonResult BuscarRubros(string prefix)
        //{
        //    //var nombres = await _provSv.BuscarAsync(new QueryFilters { Search = prefix }, TokenCookie);
        //    //var lista = nombres.Item1.Select(c => new EmpleadoVM { Nombre = c.NombreCompleto, Id = c.Id, Cuil = c.CUIT });
        //    var rub = RubroLista.Where(x => x.Rub_Desc.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
        //    var rubros = rub.Select(x => new ComboGenDto { Id = x.Rub_Id, Descripcion = x.Rub_Desc });
        //    return Json(rubros);
        //}
    }
}
