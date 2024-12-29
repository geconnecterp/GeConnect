using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;

namespace gc.sitio.Areas.ABMs.Controllers
{
    [Area("ABMs")]
    public class AbmProductoController : ProductoControladorBase
    {
        private readonly AppSettings _settings;
        private readonly IABMProductoServicio _abmProdServ;
        private readonly ICuentaServicio _ctaSv;
        private readonly IRubroServicio _rubSv;
        private readonly IProducto2Servicio _prodSv;
        private readonly IListaDePrecioServicio _listaDePrecioServicio;
        private readonly ILogger<AbmProductoController> _logger;
        private readonly IAbmServicio _abmSv;

        public AbmProductoController(IOptions<AppSettings> options, IHttpContextAccessor accessor, IABMProductoServicio productoServicio,
             ICuentaServicio cta, IRubroServicio rubro, ILogger<AbmProductoController> logger,
             IProducto2Servicio producto2Servicio, IListaDePrecioServicio listaDePrecio, IAbmServicio abmServicio) : base(options, accessor, logger)
        {
            _settings = options.Value;
            _abmProdServ = productoServicio;
            _ctaSv = cta;
            _rubSv = rubro;
            _logger = logger;
            _prodSv = producto2Servicio;
            _listaDePrecioServicio = listaDePrecio;
            _abmSv = abmServicio;
        }

        [HttpGet]
        public async Task<IActionResult> Index(bool actualizar = false)
        {
            List<ProductoListaDto> lista;
            MetadataGrid metadata;
            GridCore<ProductoListaDto> grillaDatos;

            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            CargarDatosIniciales(actualizar);



            string volver = Url.Action("index", "home", new { area = "" });
            ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

            var listR01 = new List<ComboGenDto>();
            ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
            var listR02 = new List<ComboGenDto>();
            ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BuscarBarrado(string p_id)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<ProductoBarradoDto> grillaDatos;
            try
            {
                var barr = await _prodSv.ObtenerBarradoDeProd(p_id, TokenCookie);
                if (barr == null || !barr.Ok)
                {
                    throw new NegocioException(barr.Mensaje);
                }
                else if (barr.Ok && barr.ListaEntidad.Count() == 0)
                {
                    throw new NegocioException("No se encontraron barrados para el producto.");
                }
                grillaDatos = GenerarGrilla<ProductoBarradoDto>(barr.ListaEntidad, "P_Id_barrado");
                return View("_gridBarrado", grillaDatos);
            }
            catch (NegocioException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
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
        public async Task<IActionResult> ObtenerLimiteStk(string p_id)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<LimiteStkDto> grillaDatos;
            try
            {
                var lim = await _prodSv.ObtenerLimiteStk(p_id, TokenCookie);
                if (lim == null || !lim.Ok)
                {
                    throw new NegocioException(lim.Mensaje);
                }
                else if (lim.Ok && lim.ListaEntidad.Count() == 0)
                {
                    throw new NegocioException("No se recepcionaron los limites de stock.");
                }
                grillaDatos = GenerarGrilla<LimiteStkDto>(lim.ListaEntidad, "Adm_Id");
                return View("_gridLimStk", grillaDatos);
            }
            catch (NegocioException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
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
        public IActionResult NuevoProducto()
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                string id = "000000";
                ProductoABMSeleccionado = new ProductoDto() { p_activo = 'S' };
                //busca combo familia
                ViewBag.Pg_Id = ComboProveedoresFamilia(id, _ctaSv);
                ViewBag.Up_Id = ComboMedidas(_prodSv).GetAwaiter().GetResult();
                ViewBag.Iva_Situacion = ComboIVASituacion(_prodSv).GetAwaiter().GetResult();
                ViewBag.Iva_Alicuota = ComboIVAAlicuota(_prodSv).GetAwaiter().GetResult();
                ViewBag.Lp_Id_Default = ComboListaDePrecios();

                return View("_n02panel01Producto", ProductoABMSeleccionado);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Inicializar Producto";
                _logger.LogError(ex, "Error en la invocación de la API - al Inicializar Producto");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarAbmProducto(ProductoDto prod, char accion)
        {
            try
            {
                //prod.P_Obs = prod.P_Obs.ToUpper();
                var abm = new AbmGenDto()
                {
                    Objeto = "productos",
                    Abm = accion,
                    Administracion = AdministracionId,
                    Usuario = UserName,
                    Json = JsonConvert.SerializeObject(prod)
                };
                abm.Json = abm.Json.ToLower();

                var res = await _abmSv.AbmConfirmar(abm, TokenCookie);
                if (res.Ok)
                {
                    string msg;
                    switch (accion)
                    {
                        case 'A':
                            msg = $"EL PROCESAMIENTO DEL ALTA DEL PRODUCTO {prod.p_desc} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        case 'M':
                            msg = $"EL PROCESAMIENTO DE LA MODIFICIACION DEL PRODUCTO {prod.p_desc} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        default:
                            msg = $"EL PROCESAMIENTO DE LA BAJA/DISCONTINUAR DEL PRODUCTO {prod.p_desc} SE REALIZO SATISFACTORIAMENTE";
                            break;
                    }
                    return Json(new { error = false, warn = false, msg });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.Entidad.resultado_msj, focus = res.Entidad.resultado_setfocus });
                }
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarProd(string p_id)
        {
            RespuestaGenerica<EntidadBase> response = new();

            try
            {
                var prod = await _abmProdServ.BuscarProducto(p_id, TokenCookie);
                if (prod == null)
                {
                    throw new NegocioException("No se recepcionó el producto buscado.");
                }

                ProductoABMSeleccionado = prod;
                //busca combo familia
                ViewBag.Pg_Id = ComboProveedoresFamilia(prod.cta_id, _ctaSv, prod.pg_id);
                ViewBag.Up_Id = await ComboMedidas(_prodSv);
                ViewBag.Iva_Situacion = await ComboIVASituacion(_prodSv);
                ViewBag.Iva_Alicuota = await ComboIVAAlicuota(_prodSv);
                ViewBag.Lp_Id_Default = ComboListaDePrecios();

                return View("_n02panel01Producto", prod);

            }
            catch (NegocioException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
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
            catch (Exception ex)
            {

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
                return Json(new { error = false, Metadata = MetadataProd });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, msg = "No se pudo obtener la información de paginación. Verifica" });
            }
        }

        [HttpPost]
        public JsonResult ComboProveedorFamilia(string cta_id)
        {
            try
            {
                var lista = ComboProveedoresFamilia(cta_id, _ctaSv);
                return Json(new { error = false, lista });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en {this.GetType().Name}-{MethodBase.GetCurrentMethod().Name}");
                return Json(new { error = true, msg = ex.Message });
            }
        }

        private void CargarDatosIniciales(bool actualizar)
        {
            //if (TipoNegocioLista.Count == 0 || actualizar)
            //    ObtenerTiposNegocio(_tipoNegocioServicio);

            //if (ZonasLista.Count == 0 || actualizar)
            //    ObtenerZonas(_zonaServicio);

            //if (CondicionesAfipLista.Count == 0 || actualizar)
            //    ObtenerCondicionesAfip(_condicionAfipServicio);

            //if (NaturalezaJuridicaLista.Count == 0 || actualizar)
            //    ObtenerNaturalezaJuridica(_naturalezaJuridicaServicio);

            //if (CondicionIBLista.Count == 0 || actualizar)
            //    ObtenerCondicionesIB(_condicionIBServicio);

            //if (FormaDePagoLista.Count == 0 || actualizar)
            //    ObtenerFormasDePago(_formaDePagoServicio);

            //if (ProvinciaLista.Count == 0 || actualizar)
            //    ObtenerProvincias(_provinciaServicio);

            //if (TipoCanalLista.Count == 0 || actualizar)
            //    ObtenerTiposDeCanal(_tipoCanalServicio);

            //if (TipoCuentaBcoLista.Count == 0 || actualizar)
            //    ObtenerTiposDeCuentaBco(_tipoCuentaBcoServicio);

            //if (TipoDocumentoLista.Count == 0 || actualizar)
            //    ObtenerTiposDocumento(_tipoDocumentoServicio);

            if (ListaDePreciosLista.Count == 0 || actualizar)
            {
                ObtenerListaDePrecios(_listaDePrecioServicio);
            }

            if (ProveedoresLista.Count == 0 || actualizar)
            {
                ObtenerProveedores(_ctaSv);
            }

            if (RubroLista.Count == 0 || actualizar)
            {
                ObtenerRubros(_rubSv);
            }

            //if (VendedoresLista.Count == 0 || actualizar)
            //    ObtenerListaDeVendedores(_vendedorServicio);

            //if (DiasDeLaSemanaLista.Count == 0 || actualizar)
            //    ObtenerDiasDeLaSemana();

            //if (RepartidoresLista.Count == 0 || actualizar)
            //    ObtenerListaDeRepartidores(_repartidorServicio);

            //if (TipoContactoLista.Count == 0 || actualizar)
            //    ObtenerTipoContacto(_tipoContactoServicio, "P");

            //if (TipoObservacionesLista.Count == 0 || actualizar)
            //    ObtenerTipoObservaciones(_tipoObsServicio, "P");
        }



    }
}
