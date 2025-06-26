using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
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
        private readonly IAbmServicio _abmSv;

        public AbmProductoController(IOptions<AppSettings> options, IHttpContextAccessor accessor, IABMProductoServicio productoServicio,
             ICuentaServicio cta, IRubroServicio rubro, ILogger<AbmProductoController> logger,
             IProducto2Servicio producto2Servicio, IListaDePrecioServicio listaDePrecio, IAbmServicio abmServicio) : base(options, accessor, logger)
        {
            _settings = options.Value;
            _abmProdServ = productoServicio;
            _ctaSv = cta;
            _rubSv = rubro;
            _prodSv = producto2Servicio;
            _listaDePrecioServicio = listaDePrecio;
            _abmSv = abmServicio;
        }

        [HttpGet]
        public IActionResult Index(bool actualizar = false)
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            CargarDatosIniciales(actualizar);
            ProductoBarrados = [];
            LimitesStk = [];

            //string volver = Url.Action("index", "home", new { area = "" });
            //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

            var listR01 = new List<ComboGenDto>();
            ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
            var listR02 = new List<ComboGenDto>();
            ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);
            var listR03 = new List<ComboGenDto>();
            ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);

            ViewBag.adm_id = ComboAdministraciones();
            ViewData["Titulo"] = "Gestión de Productos";
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> BuscarBarrados(string p_id)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<ProductoBarradoDto> grillaDatos;
            try
            {
                await ActualizaBarrados(p_id);
                grillaDatos = GenerarGrillaSmart(ProductoBarrados, "P_Id_barrado");
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
                string msg = "Error en la invocación de la API - Busqueda de Barrados";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda de Barrados");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        private async Task ActualizaBarrados(string p_id)
        {
            RespuestaGenerica<ProductoBarradoDto>? barr = await BuscarBarradosGen(p_id);
            if (barr == null)
            {
                throw new NegocioException("No se recepcionó el barrado buscado.");
            }
            else if (!barr.Ok)
            {
                if (string.IsNullOrEmpty(barr.Mensaje))
                {
                    throw new NegocioException("No se recepcionó el barrado buscado.");
                }
                else
                {
                    throw new NegocioException(barr.Mensaje);
                }
            }
            else if (barr.Ok && barr.ListaEntidad?.Count == 0)
            {
                throw new NegocioException("No se encontraron barrados para el producto.");
            }
            ProductoBarrados = barr.ListaEntidad;
        }

        private async Task<RespuestaGenerica<ProductoBarradoDto>?> BuscarBarradosGen(string p_id)
        {
            return await _prodSv.ObtenerBarradoDeProd(p_id, TokenCookie);
        }

        [HttpPost]
        public async Task<IActionResult> PresentarBarrado()
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<ProductoBarradoDto> grillaDatos;
            int cont = 0;
            int tope = 2;
            bool continuar = true;
            bool encontrado = false;
            try
            {
                var barr = ProductoBarrados;

                if (barr == null || barr.Count == 0)
                {
                    while (continuar)
                    {
                        var b = await BuscarBarradosGen(ProductoABMSeleccionado.p_id);

                        if (b!= null && b.Ok && b.ListaEntidad?.Count > 0)
                        {
                            ProductoBarrados = b.ListaEntidad;
                            continuar = false;
                            encontrado = true;
                        }
                        cont++;
                        if (cont > tope)
                        {
                            continuar = false;
                        }
                    }
                    if (!encontrado)
                    {
                        throw new NegocioException("No se encontraron barrados para este producto");
                    }
                }

                grillaDatos = GenerarGrillaSmart(ProductoBarrados, "P_Id_barrado");
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
                string msg = "Error la Presentar  - Busqueda de Barrados";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda de Barrados");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        /// <summary>
        /// Se buscan los datos del barrado
        /// </summary>
        /// <param name="barradoId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> BuscarBarrado(string barradoId)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                var barr = await _prodSv.ObtenerBarrado(ProductoABMSeleccionado.p_id, barradoId, TokenCookie);
                if (barr == null)
                {
                    throw new NegocioException("No se recepcionó el barrado buscado.");
                }
                else if (!barr.Ok)
                {
                    if (string.IsNullOrEmpty(barr.Mensaje))
                    {
                        throw new NegocioException("No se recepcionó el barrado buscado.");
                    }
                    throw new NegocioException(barr.Mensaje);
                }

                return Json(new { error = false, warn = false, datos = barr.Entidad });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, auth = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerLimiteStk(string p_id)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<LimiteStkDto> grillaDatos;
            try
            {
                RespuestaGenerica<LimiteStkDto>? lim = await ObtenerLimiteStkGen(p_id);
                if (lim == null || (!lim.Ok && string.IsNullOrEmpty(lim.Mensaje)))
                {
                    throw new NegocioException("No se recepcionaron los limites de stock.");
                }
                else if (!lim.Ok || !string.IsNullOrEmpty(lim.Mensaje) && !lim.Mensaje.Equals("OK",StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new NegocioException(lim.Mensaje ?? "Error desconocido al obtener los límites de stock.");
                }
                else if (lim.Ok && lim.ListaEntidad?.Count == 0)
                {
                    throw new NegocioException("No se recepcionaron los limites de stock.");
                }
                LimitesStk = lim.ListaEntidad ?? new List<LimiteStkDto>();
                grillaDatos = GenerarGrillaSmart(lim.ListaEntidad, "Adm_Id");
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
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda Producto");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        private async Task<RespuestaGenerica<LimiteStkDto>?> ObtenerLimiteStkGen(string p_id)
        {
            return await _prodSv.ObtenerLimiteStk(p_id, TokenCookie);
        }

        //
        [HttpPost]
        public async Task<IActionResult> PresentarLimiteStk()
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<LimiteStkDto> grillaDatos;
            int cont = 0;
            int tope = 2;
            bool continuar = true;
            bool encontrado = false;
            try
            {
                var lim = LimitesStk;
                if (lim.Count == 0)
                {
                    while (continuar)
                    {
                        var l = await ObtenerLimiteStkGen(ProductoABMSeleccionado.p_id);
                        if (l != null && l.Ok && l.ListaEntidad != null && l.ListaEntidad.Count > 0)
                        {
                            LimitesStk = l.ListaEntidad;
                            continuar = false;
                            encontrado = true;
                        }
                        cont++;
                        if (cont > tope)
                        {
                            continuar = false;
                        }
                    }
                    if (!encontrado)
                    {
                        throw new NegocioException("No se encontraron los limites de stock para este producto");
                    }
                }

                grillaDatos = GenerarGrillaSmart(lim, "Adm_Nombre");
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
                string msg = "Error la Presentar  - Busqueda de Barrados";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda de Barrados");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        /// <summary>
        /// Se buscan los datos del limite
        /// </summary>
        /// <param name="admId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> BuscaLimiteDato()
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                RespuestaGenerica<LimiteStkDto> lim = await _prodSv.BuscarLimite(ProductoABMSeleccionado.p_id, AdministracionId, TokenCookie);
                if (lim == null || !lim.Ok)
                {
                    // Solución para evitar el warning CS8602 y CS8604
                    if (string.IsNullOrEmpty(lim?.Mensaje))
                    {
                        throw new NegocioException("No se recepcionaron los límites de stock.");
                    }
                    else
                    {
                        throw new NegocioException(lim.Mensaje);
                    }
                }

                return Json(new { error = false, warn = false, datos = lim.Entidad });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, auth = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
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
                _logger?.LogError(ex, "Error en la invocación de la API - al Inicializar Producto");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarAbmProducto([FromBody] ProductoDto prod, char accion)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                prod = HelperGen.PasarAMayusculas(prod);
                //prod.P_Obs = prod.P_Obs.ToUpper();
                AbmGenDto abm = new AbmGenDto()
                {
                    Json = JsonConvert.SerializeObject(prod),
                    Objeto = "productos",
                    Administracion = AdministracionId,
                    Usuario = UserName,
                    Abm = accion
                };

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
                    ProductosBuscados = [];
                    if (abm.Abm.Equals('A'))
                    {
                        return Json(new { error = false, warn = false, msg, id = res.Entidad?.resultado_id });
                    }
                    return Json(new { error = false, warn = false, msg });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.Entidad?.resultado_msj, focus = res.Entidad?.resultado_setfocus });
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
        public async Task<JsonResult> ConfirmarAbmBarrado(ProductoBarradoDto barr, char accion)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                //agrego el id del producto que actulmente esta seleccionado
                barr.p_id = ProductoABMSeleccionado.p_id;

                barr = HelperGen.PasarAMayusculas(barr);
                //prod.P_Obs = prod.P_Obs.ToUpper();
                AbmGenDto abm = new AbmGenDto()
                {
                    Json = JsonConvert.SerializeObject(barr),
                    Objeto = "productos_barrados",
                    Administracion = AdministracionId,
                    Usuario = UserName,
                    Abm = accion
                };

                var res = await _abmSv.AbmConfirmar(abm, TokenCookie);
                if (res.Ok)
                {
                    string msg;
                    switch (accion)
                    {
                        case 'A':
                            msg = $"EL PROCESAMIENTO DEL ALTA DEL BARRADO {barr.p_id_barrado} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        case 'M':
                            msg = $"EL PROCESAMIENTO DE LA MODIFICIACION DEL BARRADO {barr.p_id_barrado} SE REALIZO SATISFACTORIAMENTE";

                            break;
                        default:
                            msg = $"EL PROCESAMIENTO DE LA BAJA/DISCONTINUAR DEL PRODUCTO {barr.p_id_barrado} SE REALIZO SATISFACTORIAMENTE";
                            break;
                    }
                    await ActualizaBarrados(barr.p_id);
                    return Json(new { error = false, warn = false, msg });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.Entidad?.resultado_msj, focus = res.Entidad?.resultado_setfocus });
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

        [HttpPost] //
        public async Task<JsonResult> confirmarAbmLimite(LimiteStkDto lim, char accion)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                if (string.IsNullOrEmpty(lim.adm_id))
                {
                    throw new NegocioException("No se recepcionaron datos importantes del Limite de Stock de la Sucursal. Verifique.");
                }
                if (lim.p_stk_min > lim.p_stk_max)
                {
                    throw new NegocioException("Es Stock Mínimo núnca puede ser mayor al Stock Máximo. Verifique.");
                }
                if (lim.p_stk_max < 1 || lim.p_stk_min < 1 || lim.p_stk_max > 99999 || lim.p_stk_min > 99999)
                {
                    throw new NegocioException("El Stock mínimo y el máximo siempre deben ser mayores a 1 y menores a 99999. Verifique.");
                }
                //agrego el id del producto que actulmente esta seleccionado
                lim.p_id = ProductoABMSeleccionado.p_id;

                lim = HelperGen.PasarAMayusculas(lim);
                //prod.P_Obs = prod.P_Obs.ToUpper();
                AbmGenDto abm = new AbmGenDto()
                {
                    Json = JsonConvert.SerializeObject(lim),
                    Objeto = "productos_administraciones_stk",
                    Administracion = AdministracionId,
                    Usuario = UserName,
                    Abm = accion
                };

                var res = await _abmSv.AbmConfirmar(abm, TokenCookie);
                if (res.Ok)
                {
                    string msg;
                    switch (accion)
                    {
                        case 'A':
                            msg = $"EL PROCESAMIENTO DEL ALTA DEL Limite de Stock en {lim.adm_nombre} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        case 'M':
                            msg = $"EL PROCESAMIENTO DE LA MODIFICIACION DEL BARRADO {lim.adm_nombre} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        default:
                            msg = $"EL PROCESAMIENTO DE LA BAJA/DISCONTINUAR DEL PRODUCTO {lim.adm_nombre} SE REALIZO SATISFACTORIAMENTE";
                            break;
                    }
                    return Json(new { error = false, warn = false, msg });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.Entidad?.resultado_msj, focus = res.Entidad?.resultado_setfocus });
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
                //inicializo barrados
                BarradoSeleccionado = new ProductoBarradoDto();

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
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda Producto");
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
            GridCoreSmart<ProductoListaDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (PaginaGrid == pag && !buscaNew && ProductosBuscados.Count() > 0)
                {
                    //es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
                    lista = ProductosBuscados.ToList();
                    lista = OrdenarEntidad(lista, sortDir, sort);
                    ProductosBuscados = lista;
                }
                else
                {
                    PaginaGrid = pag;
                    //traemos datos desde la base
                    query.Sort = sort;
                    query.SortDir = sortDir;
                    query.Registros = _settings.NroRegistrosPagina;
                    query.Pagina = pag;

                    var res = await _abmProdServ.BuscarProducto(query, TokenCookie);
                    lista = res.Item1 ?? [];
                    MetadataGeneral = res.Item2 ?? new MetadataGrid();
                    ProductosBuscados = lista;
                }
                metadata = MetadataGeneral;

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrillaSmart(ProductosBuscados, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

                string? volverAction = Url.Action("index", "home", new { area = "" });
                string volver = volverAction ?? "#";
                ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridAbmProds", grillaDatos);
            }
            catch (Exception ex)
            {

                string msg = "Error en la invocación de la API - Busqueda Producto";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda Producto");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
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
                _logger?.LogError(ex, $"Error en {GetType().Name}");
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
                ObtenerProveedores(_ctaSv, "BI");
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
