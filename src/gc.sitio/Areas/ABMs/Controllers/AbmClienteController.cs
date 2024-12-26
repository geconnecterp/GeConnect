using Azure;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.ABMs.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System;

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
        private readonly ILogger<AbmClienteController> _logger;

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
                                    ITipoObsServicio tipoObsServicio) : base(options, accessor, logger)
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
        }

        [HttpGet]
        public async Task<IActionResult> Index(bool actualizar = false)
        {
            MetadataGrid metadata;

            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            CargarDatosIniciales(actualizar);

            var listR02 = new List<ComboGenDto>();
            ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

            var listR01 = new List<ComboGenDto>();
            ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string sort = "cta_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
        {
            List<ABMClienteSearchDto> lista;
            MetadataGrid metadata;
            GridCore<ABMClienteSearchDto> grillaDatos;
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
                    MetadataCliente = res.Item2 ?? null;
                    metadata = MetadataCliente;
                    ClientesBuscados = lista;
                }
                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrilla(ClientesBuscados, sort, _settings.NroRegistrosPagina, pag, MetadataCliente.TotalCount, MetadataCliente.TotalPages, sortDir);

                //string volver = Url.Action("index", "home", new { area = "" });
                //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridAbmCliente", grillaDatos);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda Cliente";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda Cliente");
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
                return Json(new { error = false, Metadata = MetadataCliente });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, msg = "No se pudo obtener la información de paginación. Verifica" });
            }
        }

        /// <summary>
        /// Método que llena el Tab "Cliente" en el ABM
        /// </summary>
        /// <param name="ctaId">Valor de cuenta seleccionada en la grilla principal</param>
        /// <returns>CuentaAbmModel</returns>
        [HttpPost]
        public async Task<IActionResult> BuscarCliente(string ctaId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (string.IsNullOrEmpty(ctaId))
                    return PartialView("_tabDatosCliente", new CuentaABMDto());

                var res = _cuentaServicio.GetCuentaParaABM(ctaId, TokenCookie);
                if (res == null)
                    return PartialView("_tabDatosCliente", new CuentaABMDto());

                var cfp = _cuentaServicio.GetCuentaFormaDePago(ctaId, TokenCookie);
                var ccon = _cuentaServicio.GetCuentaContactos(ctaId, TokenCookie);
                var cobs = _cuentaServicio.GetCuentaObs(ctaId, TokenCookie);
                var cnota = _cuentaServicio.GetCuentaNota(ctaId, TokenCookie);

                var ClienteModel = new CuentaAbmModel()
                {
                    Cliente = res.First(),
                    ComboAfip = ComboAfip(),
                    ComboNatJud = ComboNatJud(),
                    ComboTipoDoc = ComboTipoDoc(),
                    ComboIngBruto = ComboIB(),
                    ComboProvincia = ComboProv(),
                    ComboDepartamento = ComboDepto(res.First().Prov_Id.ToString()),
                    ComboTipoCuentaBco = ComboTipoCuentaBco(),
                    ComboTipoNegocio = ComboTipoNegocio(),
                    ComboListaDePrecios = ComboListaDePrecios(),
                    ComboTipoCanal = ComboTipoCanal(),
                    ComboVendedores = ComboVendedores(),
                    ComboDiasDeLaSemana = ComboDiasDeLaSemana(),
                    ComboZonas = ComboZonas(),
                    ComboRepartidores = ComboRepartidores(),
                    ComboFinancieros = ComboFinanciero("BA", res.First().Cta_Emp.ToString()),
                    CuentaFormasDePago = ObtenerGridCore<CuentaFPDto>(cfp),
                    CuentaContactos = ObtenerGridCore<CuentaContactoDto>(ccon),
                    CuentaObs = ObtenerGridCore<CuentaObsDto>(cobs),
                    CuentaNota = ObtenerGridCore<CuentaNotaDto>(cnota)
                };
                return PartialView("_tabDatosCliente", ClienteModel);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Cliente";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Cliente");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        /// <summary>
        /// Método que llena el Tab "Formas de Pago" en el ABM
        /// </summary>
        /// <param name="ctaId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> BuscarFormasDePago(string ctaId)
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
                    CuentaFormasDePago = ObtenerGridCore<CuentaFPDto>(cfp),
                };
                return PartialView("_tabDatosFormaDePago", FPModel);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Formas de Pago";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Formas de Pago");
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
        public async Task<IActionResult> BuscarDatosFormasDePago(string ctaId, string fpId)
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
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Formas de Pago -> FP Selected");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        /// <summary>
        /// Método que llena el Tab "Formas de Pago" en el ABM
        /// </summary>
        /// <param name="ctaId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> BuscarOtrosContactos(string ctaId)
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
                    CuentaOtrosContactos = ObtenerGridCore<CuentaContactoDto>(coc),
                };
                return PartialView("_tabDatosOtroContacto", FPModel);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarDatosOtrosContactos(string ctaId, string tcId)
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
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> OC Selected");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarNotas(string ctaId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (string.IsNullOrEmpty(ctaId))
                    return PartialView("_tabDatosNota", new CuentaABMNotaModel());

                var cno = _cuentaServicio.GetCuentaNota(ctaId, TokenCookie);
                var FPModel = new CuentaABMNotaModel()
                {
                    CuentaNotas = ObtenerGridCore<CuentaNotaDto>(cno),
                };
                return PartialView("_tabDatosNota", FPModel);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Notas";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Notas");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarDatosNotas(string ctaId, string usuId)
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
                return PartialView("_tabDatosNotaSelected", model);

            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Notas Selected";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Notas Selected");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarObservaciones(string ctaId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (string.IsNullOrEmpty(ctaId))
                    return PartialView("_tabDatosObs", new CuentaABMObservacionesModel());

                var cob = _cuentaServicio.GetCuentaObs(ctaId, TokenCookie);
                if (cob == null)
                    return PartialView("_tabDatosObs", new CuentaABMObservacionesModel());

                var ObsModel = new CuentaABMObservacionesModel()
                {
                    ComboTipoObs = ComboTipoObservaciones(),
                    CuentaObservaciones = ObtenerGridCore<CuentaObsDto>(cob),
                };
                return PartialView("_tabDatosObs", ObsModel);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda datos TAB -> Observaciones";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Observaciones");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarDatosObservaciones(string ctaId, string toId)
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
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Obs Selected");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

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

        #region Métodos Privados
        private ObservacionesModel ObtenerObservacionModel(CuentaObsDto obs)
        {
            var mod = new ObservacionesModel();
            if (obs == null)
                return mod;

            #region map
            mod.to_lista = obs.to_lista;
            mod.to_id = obs.to_id;
            mod.to_desc = obs.to_desc;
            mod.cta_obs = obs.cta_obs;
            mod.cta_id = obs.cta_id;
            #endregion
            return mod;
        }
        private NotaModel ObtenerNotaModel(CuentaNotaDto nota)
        {
            var nom = new NotaModel();
            if (nota == null)
                return nom;

            #region map
            nom.usu_lista = nota.usu_lista;
            nom.usu_id = nota.usu_id;
            nom.usu_apellidoynombre = nota.usu_apellidoynombre;
            nom.nota = nota.nota;
            nom.fecha = nota.fecha;
            nom.cta_id = nota.cta_id;
            #endregion
            return nom;
        }
        private OtroContactoModel ObtenerOtroContactoModel(CuentaContactoDto contacto)
        {
            var ocm = new OtroContactoModel();
            if (contacto == null)
                return ocm;
            #region mapp
            ocm.tc_lista = contacto.tc_lista;
            ocm.tc_id = contacto.tc_id;
            ocm.tc_desc = contacto.tc_desc;
            ocm.cta_te = contacto.cta_te;
            ocm.cta_nombre = contacto.cta_nombre;
            ocm.cta_id = contacto.cta_id;
            ocm.cta_email = contacto.cta_email;
            ocm.cta_celu = contacto.cta_celu;
            #endregion
            return ocm;
        }

        private FormaDePagoModel ObtenerFormaDePagoModel(CuentaFPDto fp)
        {
            var fpm = new FormaDePagoModel();
            if (fp == null)
                return fpm;
            #region mapp
            fpm.fp_deufault = fp.fp_deufault;
            fpm.fp_lista = fp.fp_lista;
            fpm.fp_desc = fp.fp_desc;
            fpm.fp_id = fp.fp_id;
            fpm.cta_valores_a_nombre = fp.cta_valores_a_nombre;
            fpm.tcb_lista = fp.tcb_lista;
            fpm.cta_bco_cuenta_cbu = fp.cta_bco_cuenta_cbu;
            fpm.cta_bco_cuenta_nro = fp.cta_bco_cuenta_nro;
            fpm.cta_obs = fp.cta_obs;
            fpm.tcb_id = fp.tcb_id;
            fpm.tcb_desc = fp.tcb_desc;
            fpm.fp_dias = fp.fp_dias;
            fpm.cta_id = fp.cta_id;
            #endregion
            return fpm;
        }
        protected SelectList ComboDepto(string prov_id)
        {
            CargarDepartametos(prov_id);
            var lista = DepartamentoLista.Select(x => new ComboGenDto { Id = x.dep_id, Descripcion = x.dep_nombre });
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
                ObtenerTipoContacto(_tipoContactoServicio, "P");

            if (TipoObservacionesLista.Count == 0 || actualizar)
                ObtenerTipoObservaciones(_tipoObsServicio, "P");
        }
        #endregion
    }
}
