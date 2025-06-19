using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.ABMs.Models;
using gc.sitio.Areas.ABMs.Models.Cliente;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers
{
    [Area("ABMs")]
    public class AbmClienteController : ClienteControladorBase
    {
        private readonly AppSettings _settings;
        private readonly IABMClienteServicio _abmCliServ;
        private readonly ITipoNegocioServicio _tipoNegocioServicio;
        private readonly IZonaServicio _zonaServicio;
        private readonly ICondicionAfipServicio _condicionAfipServicio;
        private readonly INaturalezaJuridicaServicio _naturalezaJuridicaServicio;
        private readonly ICondicionIBServicio _condicionIBServicio;
        private readonly IFormaDePagoServicio _formaDePagoServicio;
        private readonly IProvinciaServicio _provinciaServicio;
        private readonly ITipoCanalServicio _tipoCanalServicio;
        private readonly ITipoCuentaBcoServicio _tipoCuentaBcoServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly ITipoDocumentoServicio _tipoDocumentoServicio;
        private readonly IDepartamentoServicio _departamentoServicio;
        private readonly IListaDePrecioServicio _listaDePrecioServicio;
        private readonly IVendedorServicio _vendedorServicio;
        private readonly IRepartidorServicio _repartidorServicio;
        private readonly IFinancieroServicio _financieroServicio;
        private readonly ITipoContactoServicio _tipoContactoServicio;
        private readonly ITipoObsServicio _tipoObsServicio;
        private readonly IAbmServicio _abmServicio;

        public AbmClienteController(IZonaServicio zonaServicio, ITipoNegocioServicio tipoNegocioServicio, IOptions<AppSettings> options,
                                    IABMClienteServicio abmClienteServicio, IHttpContextAccessor accessor,
                                    ICondicionAfipServicio condicionAfipServicio, INaturalezaJuridicaServicio naturalezaJuridicaServicio,
                                    ICondicionIBServicio condicionIBServicio, IFormaDePagoServicio formaDePagoServicio,
                                    IProvinciaServicio provinciaServicio, ITipoCanalServicio tipoCanalServicio,
                                    ITipoCuentaBcoServicio tipoCuentaBcoServicio, ICuentaServicio cuentaServicio,
                                    ITipoDocumentoServicio tipoDocumentoServicio, ILogger<AbmClienteController> logger,
                                    IDepartamentoServicio departamentoServicio, IListaDePrecioServicio listaDePrecioServicio,
                                    IVendedorServicio vendedorServicio, IRepartidorServicio repartidorServicio,
                                    IFinancieroServicio financieroServicio, ITipoContactoServicio tipoContactoServicio,
                                    ITipoObsServicio tipoObsServicio, IAbmServicio abmServicio) : base(options, accessor, logger)
        {
            _settings = options.Value;
            _tipoNegocioServicio = tipoNegocioServicio;
            _zonaServicio = zonaServicio;
            _abmCliServ = abmClienteServicio;
            _condicionAfipServicio = condicionAfipServicio;
            _naturalezaJuridicaServicio = naturalezaJuridicaServicio;
            _condicionIBServicio = condicionIBServicio;
            _formaDePagoServicio = formaDePagoServicio;
            _provinciaServicio = provinciaServicio;
            _tipoCanalServicio = tipoCanalServicio;
            _tipoCuentaBcoServicio = tipoCuentaBcoServicio;
            _cuentaServicio = cuentaServicio;
            _tipoDocumentoServicio = tipoDocumentoServicio;
            _departamentoServicio = departamentoServicio;
            _listaDePrecioServicio = listaDePrecioServicio;
            _vendedorServicio = vendedorServicio;
            _repartidorServicio = repartidorServicio;
            _financieroServicio = financieroServicio;
            _tipoContactoServicio = tipoContactoServicio;
            _tipoObsServicio = tipoObsServicio;
            _abmServicio = abmServicio;
        }

        [HttpGet]
        public IActionResult Index()
        {

            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            CargarDatosIniciales(true);

            var listR02 = new List<ComboGenDto>();
            ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

            var listR01 = new List<ComboGenDto>();
            ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

            ViewData["Titulo"] = "ABM CLIENTES";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string sort = "cta_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
        {
            List<ABMClienteSearchDto> lista;
            MetadataGrid metadata;
            GridCoreSmart<ABMClienteSearchDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (PaginaProd == pag && !buscaNew)
                {
                    //es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
                    lista = ClientesBuscados.ToList();
                    lista = OrdenarEntidad(lista, sortDir, sort);
                    ClientesBuscados = lista;
                }
                //else if (PaginaProd != pag)  //&& PaginaProd >= 0 && !query.Todo
                else
                {
                    //traemos datos desde la base
                    query.Sort = sort;
                    query.SortDir = sortDir;
                    query.Registros = _settings.NroRegistrosPagina;
                    query.Pagina = pag;

                    var res = await _abmCliServ.BuscarClientes(query, TokenCookie);
                    lista = res.Item1 ?? [];
                    MetadataGeneral = res.Item2 ?? new MetadataGrid();
                    ClientesBuscados = lista;
                }
                metadata = MetadataCliente;

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrillaSmart(ClientesBuscados, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

                //string volver = Url.Action("index", "home", new { area = "" });
                //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridAbmCliente", grillaDatos);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda Cliente";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda Cliente");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }


        }

        //[HttpPost]
        //public JsonResult ObtenerDatosPaginacion()
        //{
        //	try
        //	{
        //		return Json(new { error = false, Metadata = MetadataCliente });
        //	}
        //	catch (Exception ex)
        //	{
        //		return Json(new { error = true, msg = "No se pudo obtener la información de paginación. Verifica" });
        //	}
        //}

        [HttpPost]
        public JsonResult BuscarClienteCargado(string ctaId)
        {
            if (string.IsNullOrWhiteSpace(ctaId))
                return Json(new { error = true, warn = false, msg = "", data = "" });

            var res = _cuentaServicio.GetCuentaParaABM(ctaId, TokenCookie);
            if (res == null)
                return Json(new { error = true, warn = false, msg = "", data = "" });
            if (res.Count == 0)
                return Json(new { error = true, warn = false, msg = "", data = "" });
            return Json(new { error = false, warn = false, msg = "", data = res.First().Cta_Denominacion });
        }

        /// <summary>
        /// Método que llena el Tab "Cliente" en el ABM
        /// </summary>
        /// <param name="ctaId">Valor de cuenta seleccionada en la grilla principal</param>
        /// <returns>CuentaAbmModel</returns>
        [HttpPost]
        public IActionResult BuscarCliente(string ctaId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (string.IsNullOrEmpty(ctaId))
                    return PartialView("_tabDatosCliente", new CuentaAbmModel());

                var res = _cuentaServicio.GetCuentaParaABM(ctaId, TokenCookie);
                if (res == null || res.Count == 0)
                    return PartialView("_tabDatosCliente", new CuentaAbmModel());

                var cfp = _cuentaServicio.GetCuentaFormaDePago(ctaId, TokenCookie);
                var ccon = _cuentaServicio.GetCuentaContactos(ctaId, TokenCookie);
                var cobs = _cuentaServicio.GetCuentaObsLista(ctaId, TokenCookie);
                var cnota = _cuentaServicio.GetCuentaNota(ctaId, TokenCookie);

                var provId = res.First()?.Prov_Id?.ToString();
                var ClienteModel = new CuentaAbmModel()
                {
                    Cliente = res.First(),
                    ComboAfip = ComboAfip(),
                    ComboNatJud = ComboNatJud(),
                    ComboTipoDoc = ComboTipoDoc(),
                    ComboIngBruto = ComboIB(),
                    ComboProvincia = ComboProv(),
                    ComboDepartamento = ComboDepto(provId ?? ""),
                    ComboTipoCuentaBco = ComboTipoCuentaBco(),
                    ComboTipoNegocio = ComboTipoNegocio(),
                    ComboListaDePrecios = ComboListaDePrecios(),
                    ComboTipoCanal = ComboTipoCanal(),
                    ComboVendedores = ComboVendedores(),
                    ComboDiasDeLaSemana = ComboDiasDeLaSemana(),
                    ComboZonas = ComboZonas(),
                    ComboRepartidores = ComboRepartidores(),
                    ComboFinancieros = ComboFinanciero("BA", res.First().Cta_Emp.ToString()),
                    CuentaFormasDePago = ObtenerGridCoreSmart<CuentaFPDto>(cfp),
                    CuentaContactos = ObtenerGridCoreSmart<CuentaContactoDto>(ccon),
                    CuentaObs = ObtenerGridCoreSmart<CuentaObsDto>(cobs),
                    CuentaNota = ObtenerGridCoreSmart<CuentaNotaDto>(cnota)
                };
                return PartialView("_tabDatosCliente", ClienteModel);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Cliente";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Cliente");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        #region Formas de Pago
        /// <summary>
        /// Método que llena el Tab "Formas de Pago" en el ABM
        /// </summary>
        /// <param name="ctaId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult BuscarFormasDePago(string ctaId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (string.IsNullOrEmpty(ctaId))
                    return PartialView("_tabDatosFormaDePago", new CuentaAbmFPModel());

                var cfp = _cuentaServicio.GetCuentaFormaDePago(ctaId, TokenCookie);
                var FPModel = new CuentaAbmFPModel()
                {
                    ComboTipoCuentaBco = ComboTipoCuentaBco(),
                    ComboFormasDePago = ComboFormaDePago(),
                    CuentaFormasDePago = ObtenerGridCoreSmart<CuentaFPDto>(cfp),
                };
                return PartialView("_tabDatosFormaDePago", FPModel);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Formas de Pago";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Formas de Pago");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }


        /// <summary>
        /// Método que trae los campos de la Forma de Pago seleccionada en la grilla del tab Formas de Pago
        /// </summary>
        /// <param name="ctaId">Cuenta seleccionad en grilla principal</param>
        /// <param name="fpId">Forma de pago seleccionada en la grilla de FP del Tab Formas de Pago</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult BuscarDatosFormasDePago(string ctaId, string fpId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (string.IsNullOrWhiteSpace(ctaId) || string.IsNullOrWhiteSpace(fpId))
                    return PartialView("_tabDatosFormaDePagoSelected", new CuentaAbmFPSelectedModel());

                var cfp = _cuentaServicio.GetFormaDePagoPorCuentaYFP(ctaId, fpId, TokenCookie);
                if (cfp == null)
                    return PartialView("_tabDatosFormaDePagoSelected", new CuentaAbmFPSelectedModel());

                var FPSelectedModel = new CuentaAbmFPSelectedModel()
                {
                    ComboTipoCuentaBco = ComboTipoCuentaBco(),
                    ComboFormasDePago = ComboFormaDePago(),
                    FormaDePago = ObtenerFormaDePagoModel(cfp.First())
                };

                return PartialView("_tabDatosFormaDePagoSelected", FPSelectedModel);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Formas de Pago -> FP Selected";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Formas de Pago -> FP Selected");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
        #endregion

        #region Otros Contactos
        /// <summary>
        /// Método que llena el Tab "Formas de Pago" en el ABM
        /// </summary>
        /// <param name="ctaId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult BuscarOtrosContactos(string ctaId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (string.IsNullOrEmpty(ctaId))
                    return PartialView("_tabDatosOtroContacto", new CuentaAbmOCModel());

                var coc = _cuentaServicio.GetCuentaContactos(ctaId, TokenCookie);
                var FPModel = new CuentaAbmOCModel()
                {
                    ComboTipoContacto = ComboTipoContacto(),
                    CuentaOtrosContactos = ObtenerGridCoreSmart<CuentaContactoDto>(coc),
                };
                return PartialView("_tabDatosOtroContacto", FPModel);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public IActionResult BuscarDatosOtrosContactos(string ctaId, string tcId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (string.IsNullOrWhiteSpace(ctaId) || string.IsNullOrWhiteSpace(tcId))
                    return PartialView("_tabDatosOtroContactoSelected", new CuentaAbmOCSelectedModel());
                var coc = _cuentaServicio.GetCuentContactosporCuentaYTC(ctaId, tcId, TokenCookie);
                if (coc == null)
                    return PartialView("_tabDatosOtroContactoSelected", new CuentaAbmOCSelectedModel());
                var model = new CuentaAbmOCSelectedModel()
                {
                    ComboTipoContacto = ComboTipoContacto(),
                    OtroContacto = ObtenerOtroContactoModel(coc.First())
                };
                return PartialView("_tabDatosOtroContactoSelected", model);

            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> OC Selected";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> OC Selected");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
        #endregion

        #region Notas
        [HttpPost]
        public IActionResult BuscarNotas(string ctaId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (string.IsNullOrEmpty(ctaId))
                    return PartialView("_tabDatosNota", new CuentaABMNotaModel());

                var cno = _cuentaServicio.GetCuentaNota(ctaId, TokenCookie);
                if (cno != null && cno.Count > 0)
                {
                    cno.ForEach(x => x.usu_id_logueado = UserName);
                    cno.ForEach(x => x.puedo_editar = (x.usu_id == x.usu_id_logueado));
				}
                var FPModel = new CuentaABMNotaModel()
                {
                    CuentaNotas = ObtenerGridCoreSmart<CuentaNotaDto>(cno),
                };
                return PartialView("_tabDatosNota", FPModel);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Notas";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Notas");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public IActionResult BuscarDatosNotas(string ctaId, string usuId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (string.IsNullOrWhiteSpace(ctaId) || string.IsNullOrWhiteSpace(usuId))
                    return PartialView("_tabDatosNotaSelected", new CuentaABMNotaSelectedModel());
                var cno = _cuentaServicio.GetCuentaNotaDatos(ctaId, usuId, TokenCookie);
                if (cno == null)
                    return PartialView("_tabDatosNotaSelected", new CuentaABMNotaSelectedModel());
                var model = new CuentaABMNotaSelectedModel()
                {
                    Nota = ObtenerNotaModel(cno.First())
                };
                model.Nota.Puedo_Editar = (model.Nota.Usu_Id == UserName);
                return PartialView("_tabDatosNotaSelected", model);

            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Notas Selected";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Notas Selected");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
        #endregion

        #region Observaciones
        [HttpPost]
        public IActionResult BuscarObservaciones(string ctaId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (string.IsNullOrEmpty(ctaId))
                    return PartialView("_tabDatosObs", new CuentaABMObservacionesModel());

                var cob = _cuentaServicio.GetCuentaObsLista(ctaId, TokenCookie);
                if (cob == null)
                    return PartialView("_tabDatosObs", new CuentaABMObservacionesModel());

                var ObsModel = new CuentaABMObservacionesModel()
                {
                    ComboTipoObs = ComboTipoObservaciones(),
                    CuentaObservaciones = ObtenerGridCoreSmart<CuentaObsDto>(cob),
                };
                return PartialView("_tabDatosObs", ObsModel);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Observaciones";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Observaciones");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public IActionResult BuscarDatosObservaciones(string ctaId, string toId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (string.IsNullOrWhiteSpace(ctaId) || string.IsNullOrWhiteSpace(toId))
                    return PartialView("_tabDatosObsSelected", new CuentaABMObservacionesSelectedModel());
                var cob = _cuentaServicio.GetCuentaObsDatos(ctaId, toId, TokenCookie);
                if (cob == null)
                    return PartialView("_tabDatosObsSelected", new CuentaABMObservacionesSelectedModel());
                var model = new CuentaABMObservacionesSelectedModel()
                {
                    ComboTipoObs = ComboTipoObservaciones(),
                    Observacion = ObtenerObservacionModel(cob.First())
                };
                return PartialView("_tabDatosObsSelected", model);

            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Obs Selected";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Obs Selected");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
        #endregion

        #region Carga De Listas en sección filtros Base

        [HttpPost]
        public JsonResult BuscarR01(string prefix)
        {
            var tipoNeg = TipoNegocioLista.Where(x => x.ctn_desc.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
            var tipoNegs = tipoNeg.Select(x => new ComboGenDto { Id = x.ctn_id, Descripcion = x.ctn_lista });
            return Json(tipoNegs);
        }

        [HttpPost]
        public JsonResult BuscarR02(string prefix)
        {
            var zona = ZonasLista.Where(x => x.zn_desc.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
            var zonas = zona.Select(x => new ComboGenDto { Id = x.zn_id, Descripcion = x.zn_lista });
            return Json(zonas);
        }
        #endregion

        #region Metodo Para obtener los departamentos desde una seleccion de provincia

        [HttpPost]
        public IActionResult ObtenerDepartamentos(string provId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (string.IsNullOrWhiteSpace(provId))
                    return PartialView("_comboLocalidad", ComboDepto());
                return PartialView("_comboLocalidad", ComboDepto(provId));
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda de Departamentos";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda de Departamentos");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
        #endregion

        #region Guardado de datos
        #region Cliente
        /// <summary>
        /// Metodo que engloba las tres operaciones de ABM de cliente (Alta, baja y modificacion) invocadas al presionar ACEPTAR en la vista
        /// </summary>
        /// <param name="cuenta">Tipo CuentaAbmValidationModel</param>
        /// <param name="destinoDeOperacion"></param>
        /// <param name="tipoDeOperacion"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DataOpsCliente(CuentaAbmValidationModel cuenta, string destinoDeOperacion, char tipoDeOperacion)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                if (cuenta == null)
                {
                    throw new NegocioException("No se recepcionaron los datos confirmados para operar.");
                }
                var respuestaDeValidacion = ValidarJsonAntesDeGuardar(cuenta, tipoDeOperacion);
                if (respuestaDeValidacion == "")
                {
                    cuenta = HelperGen.PasarAMayusculas(cuenta);
                    var jsonstring = JsonConvert.SerializeObject(cuenta, new JsonSerializerSettings());
                    var respuesta = _abmServicio.AbmConfirmar(ObtenerRequestParaABM(tipoDeOperacion, destinoDeOperacion, jsonstring, AdministracionId, UserName), TokenCookie).Result;
                    return AnalizarRespuesta(respuesta);
                }
                else
                    return Json(new { error = true, warn = false, msg = respuestaDeValidacion, codigo = 1, setFocus = string.Empty });
            }
            catch(NegocioException ex)
            {
                return Json(new { error = true, msg = ex.Message});
            }
            catch 
            {
                return Json(new { error = true, msg = "Ha ocurrido un error al intentar actualizar la información." });
            }
        }

        [HttpPost]
        public IActionResult NuevoCliente()
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                var newObj = new CuentaABMDto();
                var cfp = new List<CuentaFPDto>();
                var ccon = new List<CuentaContactoDto>();
                var cobs = new List<CuentaObsDto>();
                var cnota = new List<CuentaNotaDto>();
                var ClienteModel = new CuentaAbmModel()
                {
                    Cliente = newObj,
                    ComboAfip = ComboAfip(),
                    ComboNatJud = ComboNatJud(),
                    ComboTipoDoc = ComboTipoDoc(),
                    ComboIngBruto = ComboIB(),
                    ComboProvincia = ComboProv(),
                    ComboDepartamento = ComboDepto(),
                    ComboTipoCuentaBco = ComboTipoCuentaBco(),
                    ComboTipoNegocio = ComboTipoNegocio(),
                    ComboListaDePrecios = ComboListaDePrecios(),
                    ComboTipoCanal = ComboTipoCanal(),
                    ComboVendedores = ComboVendedores(),
                    ComboDiasDeLaSemana = ComboDiasDeLaSemana(),
                    ComboZonas = ComboZonas(),
                    ComboRepartidores = ComboRepartidores(),
                    ComboFinancieros = ComboFinanciero(),
                    CuentaFormasDePago = ObtenerGridCoreSmart<CuentaFPDto>(cfp),
                    CuentaContactos = ObtenerGridCoreSmart<CuentaContactoDto>(ccon),
                    CuentaObs = ObtenerGridCoreSmart<CuentaObsDto>(cobs),
                    CuentaNota = ObtenerGridCoreSmart<CuentaNotaDto>(cnota)
                };
                return PartialView("_tabDatosCliente", ClienteModel);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Alta de Cliente - Nueva entidad";
                _logger?.LogError(ex, "Error en la invocación de la API - Alta de Cliente - Nueva entidad");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
        #endregion

        #region Forma de pago
        /// <summary>
        /// Metodo que engloba las tres operaciones de ABM de formas de pago (Alta, baja y modificacion) invocadas al presionar ACEPTAR en la vista
        /// </summary>
        /// <param name="fp">Tipo FormaDePagoAbmValidationModel</param>
        /// <param name="destinoDeOperacion"></param>
        /// <param name="tipoDeOperacion"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DataOpsFormaDePago(FormaDePagoAbmValidationModel fp, string destinoDeOperacion, char tipoDeOperacion)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                //Paso textos a mayusculas
                fp = HelperGen.PasarAMayusculas(fp);
                //Valido reglas de negocio, algunas ya se han validado en el front, pero es necsearia una re-validación para evitar introducir valores erróneos
                var respuestaDeValidacion = ValidarJsonAntesDeGuardar(fp, tipoDeOperacion);
                if (respuestaDeValidacion == "")
                {
                    //Serializo a un json string el objeto
                    var jsonstring = JsonConvert.SerializeObject(fp, new JsonSerializerSettings());
                    var respuesta = _abmServicio.AbmConfirmar(ObtenerRequestParaABM(tipoDeOperacion, destinoDeOperacion, jsonstring, AdministracionId, UserName), TokenCookie).Result;
                    return AnalizarRespuesta(respuesta);
                }
                else
                    return Json(new { error = true, warn = false, msg = respuestaDeValidacion, codigo = 1, setFocus = string.Empty });
            }
            catch 
            {
                return Json(new { error = true, msg = "Ha ocurrido un error al intentar actualizar la información." });
                throw;
            }
        }

        [HttpPost]
        public IActionResult NuevaFormaDePago()
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                var fpModel = new CuentaAbmFPSelectedModel
                {
                    FormaDePago = new FormaDePagoModel(),
                    ComboFormasDePago = ComboFormaDePago(),
                    ComboTipoCuentaBco = ComboTipoCuentaBco()
                };
                return PartialView("_tabDatosFormaDePagoSelected", fpModel);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Alta de Cliente - Nueva entidad";
                _logger?.LogError(ex, "Error en la invocación de la API - Alta de Cliente - Nueva entidad");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
        #endregion

        #region Otros Contactos
        /// <summary>
        /// Metodo que engloba las tres operaciones de ABM de Otros contactos (Alta, baja y modificacion) invocadas al presionar ACEPTAR en la vista
        /// </summary>
        /// <param name="oc">Tipo OtroContactoAbmValidationModel</param>
        /// <param name="destinoDeOperacion"></param>
        /// <param name="tipoDeOperacion"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DataOpsOtrosContactos([FromBody] OtroContactoAbmValidationModel oc, string destinoDeOperacion, char tipoDeOperacion)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                //Paso textos a mayusculas
                oc = HelperGen.PasarAMayusculas(oc);
                //Valido reglas de negocio, algunas ya se han validado en el front, pero es necsearia una re-validación para evitar introducir valores erróneos
                var respuestaDeValidacion = ValidarJsonAntesDeGuardar(oc, tipoDeOperacion);
                if (respuestaDeValidacion == "")
                {
                    //Serializo a un json string el objeto
                    var jsonstring = JsonConvert.SerializeObject(oc, new JsonSerializerSettings());
                    var respuesta = _abmServicio.AbmConfirmar(ObtenerRequestParaABM(tipoDeOperacion, destinoDeOperacion, jsonstring, AdministracionId, UserName), TokenCookie).Result;
                    return AnalizarRespuesta(respuesta);
                }
                else
                    return Json(new { error = true, warn = false, msg = respuestaDeValidacion, codigo = 1, setFocus = string.Empty });
            }
            catch 
            {
                return Json(new { error = true, msg = "Ha ocurrido un error al intentar actualizar la información." });
                throw;
            }
        }

        [HttpPost]
        public IActionResult NuevoContacto()
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                var otroContacto = new OtroContactoModel();
                var model = new CuentaAbmOCSelectedModel()
                {
                    ComboTipoContacto = ComboTipoContacto(),
                    OtroContacto = otroContacto
                };
                return PartialView("_tabDatosOtroContactoSelected", model);

            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> OC Selected";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> OC Selected");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
        #endregion

        #region Notas
        /// <summary>
        /// Metodo que engloba las tres operaciones de ABM de Notas (Alta, baja y modificacion) invocadas al presionar ACEPTAR en la vista
        /// </summary>
        /// <param name="oc">Tipo OtroContactoAbmValidationModel</param>
        /// <param name="destinoDeOperacion"></param>
        /// <param name="tipoDeOperacion"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DataOpsNotas(NotaAbmValidationModel no, string destinoDeOperacion, char tipoDeOperacion)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                var fecha = DateTime.UtcNow;
                no.fecha = fecha.ToString("yyyy-MM-dd HH:mm:ss");
                no.usu_id = UserName;
                //Paso textos a mayusculas
                no = HelperGen.PasarAMayusculas(no);
                //Valido reglas de negocio, algunas ya se han validado en el front, pero es necsearia una re-validación para evitar introducir valores erróneos
                var respuestaDeValidacion = ValidarJsonAntesDeGuardar(no, tipoDeOperacion);
                if (respuestaDeValidacion == "")
                {
                    //Serializo a un json string el objeto
                    var jsonstring = JsonConvert.SerializeObject(no, new JsonSerializerSettings());
                    var respuesta = _abmServicio.AbmConfirmar(ObtenerRequestParaABM(tipoDeOperacion, destinoDeOperacion, jsonstring, AdministracionId, UserName), TokenCookie).Result;
                    return AnalizarRespuesta(respuesta);
                }
                else
                    return Json(new { error = true, warn = false, msg = respuestaDeValidacion, codigo = 1, setFocus = string.Empty });
            }
            catch (Exception )
            {
                return Json(new { error = true, msg = "Ha ocurrido un error al intentar actualizar la información." });
                throw;
            }
        }

        [HttpPost]
        public IActionResult NuevaNota()
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                var nota = new NotaModel();
                var model = new CuentaABMNotaSelectedModel()
                {
                    Nota = nota
                };
                return PartialView("_tabDatosNotaSelected", model);

            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Notas Selected";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Notas Selected");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
        #endregion

        #region Observaciones
        /// <summary>
        /// Metodo que engloba las tres operaciones de ABM de Observaciones (Alta, baja y modificacion) invocadas al presionar ACEPTAR en la vista
        /// </summary>
        /// <param name="obs">Tipo ObservacionAbmValidationModel</param>
        /// <param name="destinoDeOperacion"></param>
        /// <param name="tipoDeOperacion"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DataOpsObservaciones(ObservacionAbmValidationModel obs, string destinoDeOperacion, char tipoDeOperacion)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                //Paso textos a mayusculas
                obs = HelperGen.PasarAMayusculas(obs);
                //Valido reglas de negocio, algunas ya se han validado en el front, pero es necsearia una re-validación para evitar introducir valores erróneos
                var respuestaDeValidacion = ValidarJsonAntesDeGuardar(obs, tipoDeOperacion);
                if (respuestaDeValidacion.mensaje == "")
                {
                    //Serializo a un json string el objeto
                    var jsonstring = JsonConvert.SerializeObject(obs, new JsonSerializerSettings());
                    var respuesta = _abmServicio.AbmConfirmar(ObtenerRequestParaABM(tipoDeOperacion, destinoDeOperacion, jsonstring, AdministracionId, UserName), TokenCookie).Result;
                    return AnalizarRespuesta(respuesta);
                }
                else
                    return Json(new { error = true, warn = false, msg = respuestaDeValidacion.mensaje, codigo = 1, setFocus = respuestaDeValidacion.setFecus });
            }
            catch (Exception )
            {
                return Json(new { error = true, msg = "Ha ocurrido un error al intentar actualizar la información." });
                throw;
            }
        }

        [HttpPost]
        public IActionResult NuevaObservacion()
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                var obs = new ObservacionesModel();
                var model = new CuentaABMObservacionesSelectedModel()
                {
                    ComboTipoObs = ComboTipoObservaciones(),
                    Observacion = obs
                };
                return PartialView("_tabDatosObsSelected", model);

            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Obs Selected";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Obs Selected");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
        #endregion

        #endregion

        #region Métodos Privados

        private RespuestaDeValidacionAntesDeGuardar ValidarJsonAntesDeGuardar(ObservacionAbmValidationModel obs, char abm)
        {

            //Agregar validaciones en el caso que lo requiera
            if (string.IsNullOrWhiteSpace(obs.cta_obs) && abm != 'B')
                return new RespuestaDeValidacionAntesDeGuardar() { mensaje = "Debe indicar una observación válida, no se permiten observaciones vacías.", setFecus = "Cta_Obs" };
            return new RespuestaDeValidacionAntesDeGuardar() { mensaje = "", setFecus = "" };
        }
        private string ValidarJsonAntesDeGuardar(NotaAbmValidationModel no, char abm)
        {
            //Agregar validaciones en el caso que lo requiera
            if (string.IsNullOrWhiteSpace(no.nota) && abm != 'B')
                return "Debe indicar un nota, no se permiten notas vacías.";
            if (string.IsNullOrWhiteSpace(no.usu_id))
                return "Debe indicar usuario válido.";
            return "";
        }
        private string ValidarJsonAntesDeGuardar(OtroContactoAbmValidationModel co, char abm)
        {
            //Agregar validaciones en el caso que lo requiera
            if (!string.IsNullOrWhiteSpace(co.cta_celu))
            {
                if (co.cta_celu.Trim().Length < 9)
                    return "El número de celular no es válido, debe tener al menos 9 digitos.";
                if (co.cta_celu.Contains(' '))
                    return "El número de celular no es válido, no debe tener espacios en blanco.";
            }
            else
                return "El número de celular no es válido, debe tener al menos 9 digitos.";
            if (!string.IsNullOrWhiteSpace(co.cta_te))
            {
                if (co.cta_te.Trim().Length < 9)
                    return "El número de teléfono no es válido, debe tener al menos 9 digitos.";
                if (co.cta_te.Contains(' '))
                    return "El número de teléfono no es válido, no debe tener espacios en blanco.";
            }
            else
                return "El número de teléfono no es válido, debe tener al menos 9 digitos.";

            if (string.IsNullOrWhiteSpace(co.cta_email))
                return "El email no es válido, debe tener la forma Ej: 'ejemplo@dominio.com'";
            if (!IsValidEmail(co.cta_email))
                return "El email no es válido, debe tener la forma Ej: 'ejemplo@dominio.com'";
            if (string.IsNullOrWhiteSpace(co.tc_id))
                return "Debe especificar un tipo de contacto.";
            return "";
        }

        private bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
        private string ValidarJsonAntesDeGuardar(FormaDePagoAbmValidationModel fp, char abm)
        {
            var abmString = abm.ToString();
            if (abmString == Abm.A.ToString() || abmString == Abm.M.ToString())
            {
                if (fp.fp_id != "B" && fp.fp_id != "I")
                {
                    fp.cta_bco_cuenta_cbu = null;
                    fp.cta_bco_cuenta_nro = null;
                }
                if (fp.fp_id != "H")
                {
                    fp.cta_valores_a_nombre = null;
                }
                if (fp.fp_dias < 0)
                {
                    return "Debe indicar un valor de Plazo mayor o igual a 0";
                }
                if (fp.tcb_lista != null && fp.tcb_lista.Contains("SELECCIONAR"))
                {
                    fp.tcb_lista = null;
                }
            }

            return "";
        }

        private string ValidarJsonAntesDeGuardar([FromBody] CuentaAbmValidationModel cuenta, char abm)
        {
            var abmString = abm.ToString();
            if (abmString == Abm.A.ToString() || abmString == Abm.M.ToString())
            {
                //En alta y modificación, solo debe cargar un régimen de IB si la condición afip es distinto de 05 y 02, en caso contrario es  nulo
                if ((cuenta.afip_id != "05" && cuenta.afip_id != "02") && cuenta.ib_id == "")
                {
                    return "Se debe indicar un valor de IB.";
                }
                //else if (cuenta.afip_id == "05" || cuenta.afip_id == "02")
                //{
                //	cuenta.ib_id = null;
                //}
                //Cantidad de PV debe ser mayor a cero si  la condición AFIP es distinto de 05 y 02
                if ((cuenta.afip_id != "05" && cuenta.afip_id != "02") && cuenta.ctac_ptos_vtas == 0)
                {
                    return "Se debe indicar un valor PV mayor a 0.";
                }
                //En las altas y modificaciones, Si el canal es “Distribuidora” (ctc_id = ‘DI’), debe ser editable el vendedor, días de visita y repartidos, en caso contrario deben ser nulos y no editables
                if (cuenta.ctc_id != "DI")
                {
                    cuenta.ve_id = null;
                    cuenta.rp_id = null;
                    cuenta.ve_visita = null;
                }
                if (cuenta.cta_emp_ctaf != null && cuenta.cta_emp_ctaf.Contains("Seleccionar"))
                    cuenta.cta_emp_ctaf = null;
            }
            return "";
        }

        private static ObservacionesModel ObtenerObservacionModel(CuentaObsDto obs)
        {
            var mod = new ObservacionesModel();
            if (obs == null)
                return mod;

            #region map
            mod.To_Lista = obs.to_lista;
            mod.To_Id = obs.to_id;
            mod.To_Desc = obs.to_desc;
            mod.Cta_Obs = obs.cta_obs;
            mod.Cta_Id = obs.cta_id;
            #endregion
            return mod;
        }
        private static NotaModel ObtenerNotaModel(CuentaNotaDto nota)
        {
            var nom = new NotaModel();
            if (nota == null)
                return nom;

            #region map
            nom.Usu_Lista = nota.usu_lista;
            nom.Usu_Id = nota.usu_id;
            nom.Usu_Apellidoynombre = nota.usu_apellidoynombre;
            nom.Nota = nota.nota;
            nom.Fecha = nota.fecha;
            nom.Cta_Id = nota.cta_id;
            #endregion
            return nom;
        }
        private static OtroContactoModel ObtenerOtroContactoModel(CuentaContactoDto contacto)
        {
            var ocm = new OtroContactoModel();
            if (contacto == null)
                return ocm;
            #region mapp
            ocm.Tc_Lista = contacto.tc_lista;
            ocm.Tc_Id = contacto.tc_id;
            ocm.Tc_Desc = contacto.tc_desc;
            ocm.Cta_Te = contacto.cta_te;
            ocm.Cta_Nombre = contacto.cta_nombre;
            ocm.Cta_Id = contacto.cta_id;
            ocm.Cta_Email = contacto.cta_email;
            ocm.Cta_Celu = contacto.cta_celu;
            #endregion
            return ocm;
        }

        private static FormaDePagoModel ObtenerFormaDePagoModel(CuentaFPDto fp)
        {
            var fpm = new FormaDePagoModel();
            if (fp == null)
                return fpm;
            #region mapp
            fpm.Fp_Deufault = fp.fp_deufault;
            fpm.Fp_Lista = fp.fp_lista;
            fpm.Fp_Desc = fp.fp_desc;
            fpm.Fp_Id = fp.fp_id;
            fpm.Cta_Valores_A_Nombre = fp.cta_valores_a_nombre;
            fpm.Tcb_Lista = fp.tcb_lista;
            fpm.Cta_Bco_Cuenta_Cbu = fp.cta_bco_cuenta_cbu;
            fpm.Cta_Bco_Cuenta_Nro = fp.cta_bco_cuenta_nro;
            fpm.Cta_Obs = fp.cta_obs;
            fpm.Tcb_Id = fp.tcb_id;
            fpm.Tcb_Desc = fp.tcb_desc;
            fpm.Fp_Dias = fp.fp_dias;
            fpm.Cta_Id = fp.cta_id;
            #endregion
            return fpm;
        }
        protected SelectList ComboDepto(string prov_id)
        {
            CargarDepartametos(prov_id);
            var lista = DepartamentoLista.Select(x => new ComboGenDto { Id = x.dep_id, Descripcion = x.dep_nombre });
            return HelperMvc<ComboGenDto>.ListaGenerica(lista);
        }
        protected SelectList ComboDepto()
        {
            var nuevaListaDpto = new List<DepartamentoDto>();
            var lista = nuevaListaDpto.Select(x => new ComboGenDto { Id = x.dep_id, Descripcion = x.dep_nombre });
            return HelperMvc<ComboGenDto>.ListaGenerica(lista);
        }
        private SelectList ComboFinanciero(string tcf_id, string cta_emp)
        {
            if (cta_emp == "S")
            {
                CargarFinancieros(tcf_id);
                var lista = FinancierosLista.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = x.ctaf_denominacion });
                return HelperMvc<ComboGenDto>.ListaGenerica(lista);
            }
            else
                return HelperMvc<ComboGenDto>.ListaGenerica(new List<FinancieroDto>().Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = x.ctaf_denominacion }));
        }
        private static SelectList ComboFinanciero()
        {
            return HelperMvc<ComboGenDto>.ListaGenerica(new List<FinancieroDto>().Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = x.ctaf_denominacion }));
        }
        private void CargarDepartametos(string prov_id)
        {
            if (DepartamentoLista == null || DepartamentoLista.Count == 0)
                ObtenerDepartamentos(_departamentoServicio, prov_id);
            else if (DepartamentoLista.First().prov_id != prov_id)
                ObtenerDepartamentos(_departamentoServicio, prov_id);
        }
        private void CargarFinancieros(string tcf_id)
        {
            if (FinancierosLista == null || FinancierosLista.Count == 0)
                ObteneFinancieros(_financieroServicio, tcf_id);
        }
        private void CargarDatosIniciales(bool actualizar)
        {
            if (TipoNegocioLista.Count == 0 || actualizar)
                ObtenerTiposNegocio(_tipoNegocioServicio);

            if (ZonasLista.Count == 0 || actualizar)
                ObtenerZonas(_zonaServicio);

            if (CondicionesAfipLista.Count == 0 || actualizar)
                ObtenerCondicionesAfip(_condicionAfipServicio);

            if (NaturalezaJuridicaLista.Count == 0 || actualizar)
                ObtenerNaturalezaJuridica(_naturalezaJuridicaServicio);

            if (CondicionIBLista.Count == 0 || actualizar)
                ObtenerCondicionesIB(_condicionIBServicio);

            if (FormaDePagoLista.Count == 0 || actualizar)
                ObtenerFormasDePago(_formaDePagoServicio);

            if (ProvinciaLista.Count == 0 || actualizar)
                ObtenerProvincias(_provinciaServicio);

            if (TipoCanalLista.Count == 0 || actualizar)
                ObtenerTiposDeCanal(_tipoCanalServicio);

            if (TipoCuentaBcoLista.Count == 0 || actualizar)
                ObtenerTiposDeCuentaBco(_tipoCuentaBcoServicio);

            if (TipoDocumentoLista.Count == 0 || actualizar)
                ObtenerTiposDocumento(_tipoDocumentoServicio);

            if (ListaDePreciosLista.Count == 0 || actualizar)
                ObtenerListaDePrecios(_listaDePrecioServicio);

            if (VendedoresLista.Count == 0 || actualizar)
                ObtenerListaDeVendedores(_vendedorServicio);

            if (DiasDeLaSemanaLista.Count == 0 || actualizar)
                ObtenerDiasDeLaSemana();

            if (RepartidoresLista.Count == 0 || actualizar)
                ObtenerListaDeRepartidores(_repartidorServicio);

            if (TipoContactoLista.Count == 0 || actualizar)
                ObtenerTipoContacto(_tipoContactoServicio, "C");

            if (TipoObservacionesLista.Count == 0 || actualizar)
                ObtenerTipoObservaciones(_tipoObsServicio, "C");
        }
        #endregion
    }
}
