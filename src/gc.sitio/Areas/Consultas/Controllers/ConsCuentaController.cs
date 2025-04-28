using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.DocManager;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace gc.sitio.Areas.Consultas.Controllers
{
    /// <summary>
    /// Corresponde el controlador a las CUENTAS COMERCIALES
    /// </summary>
    [Area("Consultas")]
    public class ConsCuentaController : ConsultaControladorBase
    {
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.CCUENTAS.ToString();

        private readonly IOptions<AppSettings> options1;
        private readonly AppSettings _settings;
        private readonly IConsultasServicio _consSv;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IDocManagerServicio _docMSv;
        private readonly IWebHostEnvironment _env;
        private readonly EmpresaGeco _empresaGeco;


        public ConsCuentaController(IOptions<AppSettings> options, IHttpContextAccessor contexto,
            ILogger<ConsCuentaController> logger, IConsultasServicio consulta,
            IOptions<DocsManager> docsManager, ICuentaServicio cuentaServicio,
            IDocManagerServicio docManager, IWebHostEnvironment env, IOptions<EmpresaGeco> empresa) : base(options, contexto, logger)
        {
            _consSv = consulta;
            options1 = options;
            _settings = options.Value;
            _cuentaServicio = cuentaServicio;
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManager;
            _env = env;
            _empresaGeco = empresa.Value;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }
                //se genera una lista vacia para hidratar el componente que contendrá 
                //la seleccion de la cuenta
                var listR01 = new List<ComboGenDto>();
                ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
                string titulo = "Consulta de Cuentas Comerciales";
                ViewData["Titulo"] = titulo;

                #region INICIALIZACION DE VARIABLES DE SESSION
                CuentaCorrienteBuscada = [];
                VencimientosBuscados = [];
                CmptesTotalBuscados = [];
                CmptesDetalleBuscados = [];
                OrdenPagosBuscados = [];
                OrdenPagosDetBuscados = [];
                RecepProvBuscados = [];
                RecepProvDetBuscados = [];
                ModuloDM = "Consultas Cta Cte";
                #region Gestor Impresion - Inicializacion de variables

                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);

                //en este mismo acto se cargan los posibles documentos
                //que se pueden imprimir, exportar, enviar por email o whatsapp
                ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion


                #endregion

                return View();
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Index", "home", new { area = "" });

            }
        }

        /// <summary>
        /// Acción que permite consultar la cuenta corriente de un cliente o proveedor.
        /// </summary>
        /// <param name="ctaId">Identificador de la cuenta a consultar.</param>
        /// <param name="fechaD">Fecha desde la cual se realiza la consulta.</param>
        /// <param name="buscaNew">Indica si se debe realizar una nueva búsqueda.</param>
        /// <param name="sort">Campo por el cual se ordenarán los resultados.</param>
        /// <param name="sortDir">Dirección de ordenamiento (ascendente o descendente).</param>
        /// <param name="pag">Número de página a consultar.</param>
        /// <returns>Devuelve una vista parcial con los datos de la cuenta corriente o un mensaje de error en caso de fallo.</returns>
        /// <remarks>
        /// Este método realiza las siguientes operaciones:
        /// 1. Verifica si la página solicitada ya fue cargada previamente para evitar consultas innecesarias.
        /// 2. Si es una nueva consulta, invoca el servicio para obtener los datos de la cuenta corriente.
        /// 3. Ordena los datos obtenidos según los parámetros proporcionados.
        /// 4. Genera un reporte en base a los datos consultados y lo marca como consultado en el gestor de impresión.
        /// 5. Devuelve una vista parcial con los datos organizados en una grilla.
        /// </remarks>
        /// <exception cref="NegocioException">Se lanza si no se encuentran datos de la cuenta o si ocurre un error de negocio.</exception>
        /// <exception cref="Exception">Se lanza si ocurre un error general durante la ejecución.</exception>
        [HttpPost]
        public async Task<IActionResult> ConsultarCuentaCorriente(string ctaId, DateTime fechaD, bool buscaNew, string sort = "p_desc", string sortDir = "asc", int pag = 1)
        {

            //la misma que esta especificado en el appsettings.json

            List<ConsCtaCteDto> lista;

            GridCoreSmart<ConsCtaCteDto> grillaDatos;

            RespuestaGenerica<EntidadBase> response = new();

            MetadataGrid metadata;
            try

            {
                if (PaginaGrid == pag && !buscaNew && CuentaCorrienteBuscada.Count() > 0)

                {

                    //es la misma pagina y hay registros, se realiza el reordenamiento de los datos.

                    lista = CuentaCorrienteBuscada.ToList();

                    lista = OrdenarEntidad(lista, sortDir, sort);

                    CuentaCorrienteBuscada = lista;

                }

                else

                {

                    PaginaGrid = pag;

                    int registros = _settings.NroRegistrosPagina;



                    var res = await _consSv.ConsultarCuentaCorriente(ctaId, fechaD.Ticks, UserName, pag, registros, TokenCookie);

                    lista = res.Item1 ?? [];

                    MetadataGeneral = res.Item2 ?? new MetadataGrid();

                    CuentaCorrienteBuscada = lista;





                    #region Consulta de Cuenta comercial
                    var cuenta = await _cuentaServicio.ObtenerListaCuentaComercial(ctaId, 'T', TokenCookie);

                    if (cuenta == null || cuenta.Count == 0)

                    {

                        throw new NegocioException("No se encontraron los datos de la cuenta. Verifique conexión.");

                    }

                    CuentaComercialSeleccionada = cuenta.First();


                    #region Obtener datos de la cuenta
                    var datosCta = await _cuentaServicio.ObtenerCuentaDatos(ctaId,'D', TokenCookie);
                    if (datosCta == null || datosCta.Count == 0)
                    {
                        throw new NegocioException("No se encontraron los datos de la cuenta. Verifique conexión.");
                    }
                    CuentaComercialDatosSeleccionada = datosCta.FirstOrDefault();
                    #endregion

                    #endregion

                    if (lista.Count > 0)

                    {

                        var consulta = AppReportes.CCUENTAS_CUENTA_CORRIENTE;

                        #region Gestor Impresion - marcando que hay datos para el reporte n
                        var reportes = ArchivosCargadosModulo;

                        string tipoDato = CuentaCorrienteBuscada?.GetType().FullName ?? "";

                        var mod = _docsManager.Modulos?.Find(x => x.Id.Equals(APP_MODULO))?.Reportes.Find(x => x.Id == (int)consulta);

                        var titulo = mod?.Titulos[0];

                        string archb64 = GenerarArchivoB64(lista, consulta, CuentaComercialSeleccionada.Prov_Id, titulo ?? "");

                        //el 3er parametro es el numero de reporte que se esta marcando como consultado

                        reportes = _docMSv.MarcarConsultaRealizada(reportes, consulta, 1, archb64, tipoDato);

                        ArchivosCargadosModulo = reportes;

                        #endregion
                    }
                }

                metadata = MetadataGeneral;

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.

                grillaDatos = GenerarGrillaSmart(CuentaCorrienteBuscada, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

                return View("_gridCtaCte", grillaDatos);

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

                string msg = "Error en la invocación de la API - Consulta de Cuenta Corriente.";

                _logger?.LogError(ex, msg);

                response.Mensaje = msg;

                response.Ok = false;

                response.EsWarn = false;

                response.EsError = true;

                return PartialView("_gridMensaje", response);

            }

        }

        /// <summary>
        /// Acción que permite consultar los vencimientos de comprobantes no imputados de una cuenta comercial.
        /// </summary>
        /// <param name="ctaId">Identificador de la cuenta a consultar.</param>
        /// <param name="fechaD">Fecha desde la cual se realiza la consulta.</param>
        /// <param name="fechaH">Fecha hasta la cual se realiza la consulta.</param>
        /// <returns>
        /// Devuelve una vista parcial con los datos de los vencimientos organizados en una grilla.
        /// En caso de error, devuelve un mensaje de error en una vista parcial.
        /// </returns>
        /// <remarks>
        /// Este método realiza las siguientes operaciones:
        /// 1. Invoca el servicio para obtener los datos de vencimientos de comprobantes no imputados.
        /// 2. Si se obtienen datos, los organiza en una grilla y los marca como consultados en el gestor de impresión.
        /// 3. Devuelve una vista parcial con los datos organizados o un mensaje de error en caso de fallo.
        /// </remarks>
        /// <exception cref="NegocioException">
        /// Se lanza si ocurre un error de negocio, como la falta de datos consultados.
        /// </exception>
        /// <exception cref="Exception">
        /// Se lanza si ocurre un error general durante la ejecución.
        /// </exception>
        public async Task<IActionResult> ConsultarVencimiento(string ctaId, DateTime fechaD, DateTime fechaH)

        {
            GridCoreSmart<ConsVtoDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            //la misma que esta especificado en el appsettings.json
            var consulta = AppReportes.CCUENTAS_VENCIMIENTO_COMPROBANTES;
            try
            {
                //voy a invocar el servicio que me traiga los datos de la cuenta corriente 
                //para la cuenta seleccionada
                var res = await _consSv.ConsultaVencimientoComprobantesNoImputados(ctaId, fechaD.Ticks, fechaH.Ticks, UserName, TokenCookie);
                string sort = string.Empty;
                string sortDir = string.Empty;
                int pag = 1;


                if (!res.Ok)
                {
                    if (res.Mensaje == null)
                    {
                        throw new Exception("No se pudo obtener los datos consultados de vencimientos");
                    }
                    throw new NegocioException(res.Mensaje);
                }
                VencimientosBuscados = res.ListaEntidad ?? new List<ConsVtoDto>();

                if (res.ListaEntidad?.Count > 0)
                {
                    #region Gestor Impresion - marcando que hay datos para el reporte n
                    var reportes = ArchivosCargadosModulo;
                    string tipoDato = VencimientosBuscados.GetType().FullName ?? "";

                    var mod = _docsManager.Modulos?.Find(x => x.Id.Equals(APP_MODULO))?.Reportes.Find(x => x.Id == (int)consulta);
                    var titulo = mod?.Titulos[0];
                    string archb64 = GenerarArchivoB64(res.ListaEntidad, consulta, CuentaComercialSeleccionada?.Prov_Id, titulo ?? "");

                    //el 3er parametro es el numero de reporte que se esta marcando como consultado
                    reportes = _docMSv.MarcarConsultaRealizada(reportes, consulta, 1, archb64, tipoDato);
                    ArchivosCargadosModulo = reportes;
                    #endregion
                }

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrillaSmart(res.ListaEntidad, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

                //string volver = Url.Action("index", "home", new { area = "" });
                //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridVencimiento", grillaDatos);
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

                string msg = "Error en la invocación de la API - Consulta de Vencimiento.";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        public async Task<IActionResult> ConsultarCmpteTotal(string ctaId, int meses, bool relCuil)
        {
            GridCoreSmart<ConsCompTotDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                //voy a invocar el servicio que me traiga los datos de la cuenta corriente 
                //para la cuenta seleccionada
                var res = await _consSv.ConsultaComprobantesMeses(ctaId, meses, relCuil, UserName, TokenCookie);
                string sort = string.Empty;
                string sortDir = string.Empty;
                int pag = 1;

                if (!res.Ok)
                {
                    throw new NegocioException(res.Mensaje ?? "");
                }
                CmptesTotalBuscados = res.ListaEntidad ?? [];

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrillaSmart(res.ListaEntidad, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

                return View("_gridCmptTot", grillaDatos);
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

                string msg = "Error en la invocación de la API - Consulta de Comprobantes por TOTALES.";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        public async Task<IActionResult> ConsultarCmpteDetalle(string ctaId, string mes, bool relCuil)
        {
            GridCoreSmart<ConsCompDetDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();

            //INICIALMENTE SE INDICA QUE EL REPORTE A GENERAR ES EL DE DETALLE DE COMPROBANTES
            //ESTO ES ORIENTADO A LA GENERACION DE ARCHIVOS B64 PARA EL GESTOR DE IMPRESION
            var consultaPpal = AppReportes.CCUENTAS_COMPROBANTES;
            var consulta = AppReportes.CCUENTAS_COMPROBANTES_DETALLE;

            try
            {
                //voy a invocar el servicio que me traiga los datos de la cuenta corriente 
                //para la cuenta seleccionada
                var res = await _consSv.ConsultaComprobantesMesDetalle(ctaId, mes, relCuil, UserName, TokenCookie);
                string sort = string.Empty;
                string sortDir = string.Empty;
                int pag = 1;

                if (!res.Ok)
                {
                    throw new NegocioException(res.Mensaje ?? "No se encontraron registros para el detalle");
                }
                CmptesDetalleBuscados = res.ListaEntidad ?? [];

                if (res.ListaEntidad?.Count > 0)
                {
                    #region Gestor Impresion - marcando que hay datos para el reporte n
                    var reportes = ArchivosCargadosModulo;
                    string tipoDato = CmptesDetalleBuscados.GetType().FullName ?? "";
                    var mod = (_docsManager?.Modulos ?? []).Find(x => x.Id.Equals(APP_MODULO))?.Reportes.Find(x => x.Id == (int)consultaPpal);
                    var titulo = mod?.Titulos[0];
                    string archb64 = GenerarArchivoB64(res.ListaEntidad, consulta, CuentaComercialSeleccionada?.Prov_Id, titulo ?? "");

                    //ACA SE CAMBIA LA VARIABLE CONSULTA PARA INDICAR EL MODULO GENERAL
                    consulta = AppReportes.CCUENTAS_COMPROBANTES;
                    //el 3er parametro es el numero de reporte que se esta marcando como consultado
                    reportes = _docMSv.MarcarConsultaRealizada(reportes, consulta, 2, archb64, tipoDato);

                    ArchivosCargadosModulo = reportes;
                    #endregion
                }

                string provId = CuentaComercialSeleccionada?.Prov_Id.ToString() ?? "";
                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrillaSmart(res.ListaEntidad, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
                grillaDatos.DatoAux01 = provId;

                return View("_gridCmptDet", grillaDatos);
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

                string msg = $"Error en la invocación de la API - Consulta de Detalle de Comprobantes del Periodo {mes.Substring(4, 2)}-{mes.Substring(0, 4)}.";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConsultaOPagoProveedor(string ctaId, DateTime fechaD, DateTime fechaH)
        {
            GridCoreSmart<ConsOrdPagosDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                //voy a invocar el servicio que me traiga los datos de la cuenta corriente 
                //para la cuenta seleccionada
                var res = await _consSv.ConsultaOrdenesDePagoProveedor(ctaId, fechaD, fechaH, "%", "%", TokenCookie);
                string sort = string.Empty;
                string sortDir = string.Empty;
                int pag = 1;


                if (!res.Ok)
                {
                    throw new NegocioException(res.Mensaje ?? "No se encontraron las ordenes de pago solicitadas");
                }
                OrdenPagosBuscados = res.ListaEntidad ?? [];
                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrillaSmart(res.ListaEntidad, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

                //string volver = Url.Action("index", "home", new { area = "" });
                //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridOPagoProv", grillaDatos);
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

                string msg = "Error en la invocación de la API - Consulta de Ordenes de Pago.";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConsultaOPagoProveedorDetalle(string cmptId)
        {
            GridCoreSmart<ConsOrdPagosDetDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            var consultaPpal = AppReportes.CCUENTAS_ORDEN_DE_PAGO;
            var consulta = AppReportes.CCUENTAS_ORDEN_DE_PAGO_DETALLE;
            try
            {
                //voy a invocar el servicio que me traiga los datos de la cuenta corriente 
                //para la cuenta seleccionada
                var res = await _consSv.ConsultaOrdenesDePagoProveedorDetalle(cmptId, TokenCookie);
                string sort = string.Empty;
                string sortDir = string.Empty;
                int pag = 1;


                if (!res.Ok)
                {
                    throw new NegocioException(res.Mensaje ?? "No se rescepcionaron los registros de Pago a Proveedores.");
                }
                OrdenPagosDetBuscados = res.ListaEntidad ?? [];

                if (res.ListaEntidad?.Count > 0)
                {
                    #region Gestor Impresion - marcando que hay datos para el reporte n
                    var reportes = ArchivosCargadosModulo;
                    string tipoDato = OrdenPagosDetBuscados.GetType().FullName ?? string.Empty;
                    var mod = (_docsManager.Modulos ?? []).Find(x => x.Id.Equals(APP_MODULO))?.Reportes.Find(x => x.Id == (int)consultaPpal);
                    var titulo = mod?.Titulos[0];
                    string archb64 = GenerarArchivoB64(res.ListaEntidad, consulta, CuentaComercialSeleccionada?.Prov_Id, titulo ?? "");
                    //el 3er parametro es el numero de reporte que se esta marcando como consultado
                    consulta = AppReportes.CCUENTAS_ORDEN_DE_PAGO;
                    reportes = _docMSv.MarcarConsultaRealizada(reportes, consulta, 2, archb64, tipoDato);

                    ArchivosCargadosModulo = reportes;
                    #endregion
                }

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrillaSmart(res.ListaEntidad, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

                //string volver = Url.Action("index", "home", new { area = "" });
                //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridOPagoProvDet", grillaDatos);
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

                string msg = "Error en la invocación de la API - Consulta del Detalle de Ordenes de Pago.";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConsultaRecepcionProveedor(string ctaId, DateTime fechaD, DateTime fechaH)
        {
            GridCoreSmart<ConsRecepcionProveedorDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                //voy a invocar el servicio que me traiga los datos de la cuenta corriente 
                //para la cuenta seleccionada
                var res = await _consSv.ConsultaRecepcionProveedor(ctaId, fechaD, fechaH, AdministracionId, TokenCookie);
                string sort = string.Empty;
                string sortDir = string.Empty;
                int pag = 1;


                if (!res.Ok)
                {
                    throw new NegocioException(res.Mensaje ?? "No se logro obtener datos de Recepcion de Proveedores");
                }
                var lista = res.ListaEntidad?.OrderByDescending(x => x.Rp_fecha).ToList();
                RecepProvBuscados = lista ?? [];

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrillaSmart(lista, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

                //string volver = Url.Action("index", "home", new { area = "" });
                //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridRecProv", grillaDatos);
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

                string msg = "Error en la invocación de la API - Consulta de Recepciones del Proveedor.";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConsultaRecepcionProveedorDetalle(string cmptId)
        {
            GridCoreSmart<ConsRecepcionProveedorDetalleDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            var consultaPpal = AppReportes.CCUENTAS_RECEPCION_PROVEEDORES;
            var consulta = AppReportes.CCUENTAS_RECEPCION_PROVEEDORES_DETALLE;

            try
            {
                //voy a invocar el servicio que me traiga los datos de la cuenta corriente 
                //para la cuenta seleccionada
                var res = await _consSv.ConsultaRecepcionProveedorDetalle(cmptId, TokenCookie);
                string sort = string.Empty;
                string sortDir = string.Empty;
                int pag = 1;


                if (!res.Ok)
                {
                    throw new NegocioException(res.Mensaje ?? "No se encontró el detalle de las recepciones de proveedores.");
                }
                RecepProvDetBuscados = res.ListaEntidad ?? [];

                if (res.ListaEntidad?.Count > 0)
                {
                    #region Gestor Impresion - marcando que hay datos para el reporte n
                    var reportes = ArchivosCargadosModulo;
                    string tipoDato = RecepProvDetBuscados.GetType().FullName ?? "";
                    var mod = (_docsManager.Modulos ?? []).Find(x => x.Id.Equals(APP_MODULO))?.Reportes.Find(x => x.Id == (int)consultaPpal);
                    var titulo = mod?.Titulos[0];
                    string archb64 = GenerarArchivoB64(res.ListaEntidad, consulta, CuentaComercialSeleccionada?.Prov_Id, titulo ?? "");

                    consulta = AppReportes.CCUENTAS_RECEPCION_PROVEEDORES;
                    //el 3er parametro es el numero de reporte que se esta marcando como consultado (1=original). 
                    //se deberá ver cuando se tenga que imprimir un duplicado.deberia ser 2 o 3 o lo que corresponda.
                    reportes = _docMSv.MarcarConsultaRealizada(reportes, consulta, 2, archb64, tipoDato);

                    ArchivosCargadosModulo = reportes;
                    #endregion
                }

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrillaSmart(res.ListaEntidad, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

                //string volver = Url.Action("index", "home", new { area = "" });
                //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridRecProvDet", grillaDatos);
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

                string msg = "Error en la invocación de la API - Consulta del Detalle de Recepciones.";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        #region Metodos Privados
        private string GenerarArchivoB64<T>(List<T> lista, AppReportes consulta, char? provId, string titulo)
        {
            MemoryStream ms = new();
            string observacion = string.Empty;
            List<string> titulos;
            float[] anchos;
            switch (consulta)
            {
                case AppReportes.CCUENTAS_CUENTA_CORRIENTE:
                    observacion = "CUENTA CORRIENTE - Documento generado por el sistema de Gestión Comercial";
                    var cDatos = (lista as List<ConsCtaCteDto> ?? []).Select(x => new
                    {
                        Fecha = x.Cc_fecha.ToShortDateString(),
                        DiaMovi = x.Dia_movi,
                        Comprobante = $"{x.Tco_desc}-{x.Cm_compte}",
                        Concepto = x.Cc_concepto,
                        Debe = x.Cc_debe,
                        Haber = x.Cc_haber,
                        Saldo = x.Cc_saldo
                    }).ToList();

                    titulos = new List<string> { "Fecha", "N° Mov", "Comprobante", "Concepto", "Debe", "Haber", "Saldo" };
                    anchos = [10f, 10f, 20f, 30f, 10f, 10f, 10f];
                    GeneraReporteSegunDatos(consulta, cDatos, observacion, out ms, titulos, anchos, true, titulo);

                    break;
                case AppReportes.CCUENTAS_VENCIMIENTO_COMPROBANTES:
                    observacion = "VENCIMIENTOS DE COMPROBANTES - Documento generado por el sistema de Gestión Comercial";
                    var vDatos = (lista as List<ConsVtoDto> ?? []).Select(x => new
                    {
                        Descripcion = $"{x.Tco_desc}-{x.Cm_compte}",
                        Cuota = x.Cm_compte_cuota,
                        Est = x.Cv_estado,
                        FechaCmpte = x.Cv_fecha_carga.ToShortDateString(),
                        FechaVto = x.Cv_fecha_vto.ToShortDateString(),
                        Importe = x.Cv_importe
                    }).ToList();

                    titulos = new List<string> { "Descripcion", "Cuota", "Est.", "Fecha Comp.", "Fecha Vto", "Importe" };
                    anchos = [50f, 10f, 10f, 10f, 10f, 10f];

                    GeneraReporteSegunDatos(consulta, vDatos, observacion, out ms, titulos, anchos, true, titulo);
                    break;
                case AppReportes.CCUENTAS_COMPROBANTES:
                    break;
                case AppReportes.CCUENTAS_ORDEN_DE_PAGO:
                    break;
                case AppReportes.CCUENTAS_RECEPCION_PROVEEDORES:
                    break;
                case AppReportes.CCUENTAS_COMPROBANTES_DETALLE:
                    observacion = "DETALLE DE COMPROBANTES - Documento generado por el sistema de Gestión Comercial";
                    if (provId.Equals('P'))
                    {
                        var cdDatos = (lista as List<ConsCompDetDto> ?? []).Select(x => new
                        {
                            Fecha = x.Cm_fecha.ToShortDateString(),
                            NroComp = $"({x.Tco_id}){x.Cm_compte}",
                            Neto = x.Cm_neto,
                            IVA = x.Cm_iva,
                            TOTAL = x.Cm_total,
                            NroIdent = x.Op_compte,
                            Cmpte = $"({x.Tco_id}){x.Cm_compte}",
                            Cargado = x.Cm_fecha_carga.ToShortDateString(),
                            Usu = x.Usu_id
                        }).ToList();


                        titulos = new List<string> { "Fecha", "N° Cmpte", "Neto", "IVA", "TOTAL", "N° Or.Pago", "Cmpte ORI", "CARGADO", "USUARIO" };
                        anchos = [10f, 15f, 10f, 10f, 10f, 15f, 10f, 10f, 10f];

                        GeneraReporteSegunDatos(consulta, cdDatos, observacion, out ms, titulos, anchos, true, titulo);
                    }
                    else
                    {
                        var cdDatos = (lista as List<ConsCompDetDto> ?? []).Select(x => new
                        {
                            Fecha = x.Cm_fecha.ToShortDateString(),
                            NroComp = $"({x.Tco_id}){x.Cm_compte}",
                            Neto = x.Cm_neto,
                            IVA = x.Cm_iva,
                            TOTAL = x.Cm_total,
                            NroIdent = x.Doc_compte,
                            Cmpte = $"({x.Tco_id_ori}){x.Cm_compte_ori}",
                            Cargado = x.Cm_fecha_carga.ToShortDateString(),
                            Usu = x.Usu_id
                        }).ToList();


                        titulos = new List<string> { "Fecha", "N° Cmpte", "Neto", "IVA", "TOTAL", "N° DOC", "Cmpte ORI", "CARGADO", "USUARIO" };
                        anchos = [10f, 15f, 10f, 10f, 10f, 15f, 10f, 10f, 10f];

                        GeneraReporteSegunDatos(consulta, cdDatos, observacion, out ms, titulos, anchos, true, titulo);
                    }


                    break;
                case AppReportes.CCUENTAS_ORDEN_DE_PAGO_DETALLE:
                    observacion = "DETALLE DE ORDENES DE PAGO - Documento generado por el sistema de Gestión Comercial";

                    var opDatos = (lista as List<ConsOrdPagosDetDto> ?? [])
                                    .OrderBy(x => x.Grupo) // Ordenar por el campo Grupo
                                    .Select(x => new
                                    {
                                        x.Grupo,
                                        GrupoDesc = x.Grupo_des,
                                        x.Concepto,
                                        Importe = x.Cc_importe
                                    }).ToList();

                    //var opDatos = (lista as List<ConsOrdPagosDetDto>)
                    //    .GroupBy(x => new { x.Grupo, x.Grupo_des })
                    //    .Select(g => new
                    //    {
                    //        Group = g.Key.Grupo,
                    //        GrupoDesc = g.Key.Grupo_des,
                    //        Items = g.Select(item => new
                    //        {
                    //            item.Concepto,
                    //            item.Cc_importe,
                    //        }).ToList()

                    //    }).ToList();

                    titulos = new List<string> { "Grupo", "Descripcion", "Concepto", "Importe" };
                    anchos = [5f, 50f, 25f, 20f];

                    GeneraReporteSegunDatos(consulta, opDatos, observacion, out ms, titulos, anchos, true, titulo);
                    break;
                case AppReportes.CCUENTAS_RECEPCION_PROVEEDORES_DETALLE:

                    break;
                default:
                    break;
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        private void GeneraReporteSegunDatos<S>(AppReportes consulta, List<S> vDatos, string observacion, out MemoryStream ms, List<string> titulos, float[] anchos, bool datosCliente, string titulo)
        {
            var cuenta = CuentaComercialSeleccionada;

            PrintRequestDto<S> request = new PrintRequestDto<S>();
            string logoPath = Path.Combine(_env.WebRootPath, "img", "gc.png");
            if (cuenta == null || string.IsNullOrEmpty(cuenta.Cta_Id))
            {
                throw new NegocioException("No se detecto la cuenta. Verifique. debería refrescar la vista.");
            }

            request.Cabecera = new DatosCabeceraDto
            {
                NombreEmpresa = _empresaGeco.Nombre,
                Direccion = _empresaGeco.Direccion,
                CUIT = _empresaGeco.CUIT,
                IIBB = _empresaGeco.IngresosBrutos,
                Sucursal = AdministracionId,
                TituloDocumento = $"{titulo}_{cuenta?.Cta_Id}_{DateTime.Now.Ticks}",
                Logo = logoPath,
            };
            request.Pie = new DatosPieDto
            {
                Usuario = UserName,
                Observaciones = observacion
            };
            request.Cuerpo = new DatosCuerpoDto<S>
            {
                CtaId = cuenta?.Cta_Id ?? "",
                RazonSocial = cuenta?.Cta_Denominacion ?? "",
                Domicilio = cuenta?.Cta_Domicilio ?? "",
                CUIT = cuenta?.Cta_Documento ?? "",
                Contacto = "-",
                Datos = vDatos,
            };

            _docMSv.GenerarArchivoPDF(request, out ms, titulos, anchos, datosCliente);

        }

        private async Task<CuentaDto> BuscarCuentaComercial(string ctaId, char t)
        {
            List<CuentaDto> Lista = new();
            Lista = await _cuentaServicio.ObtenerListaCuentaComercial(ctaId, t, TokenCookie);
            if (Lista.Count > 0)
            {
                foreach (var item in Lista)
                {
                    RegexOptions options = RegexOptions.None;
                    Regex regex = new("[ ]{2,}", options);
                    item.Cta_Denominacion = regex.Replace(item.Cta_Denominacion, " ");
                }
                return Lista.FirstOrDefault() ?? new();
            }
            return new CuentaDto();
        }

        #endregion
    }
}
