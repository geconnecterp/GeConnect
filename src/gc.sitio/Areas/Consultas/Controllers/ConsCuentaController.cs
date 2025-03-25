using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.DocManager;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
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
        private readonly ILogger<ConsCuentaController> _logger;
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
            _logger = logger;
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
                ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo.Reportes); ;

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

        [HttpPost]
        public async Task<IActionResult> ConsultarCuentaCorriente(string ctaId, DateTime fechaD, bool buscaNew, string sort = "p_desc", string sortDir = "asc", int pag = 1)
        {
            //la misma que esta especificado en el appsettings.json

            List<ConsCtaCteDto> lista;
            GridCore<ConsCtaCteDto> grillaDatos;
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
                    #endregion

                    if (lista.Count > 0)
                    {
                        var consulta = AppReportes.CCUENTAS_CUENTA_CORRIENTE;
                        #region Gestor Impresion - marcando que hay datos para el reporte n
                        var reportes = ArchivosCargadosModulo;
                        string archb64 = GenerarArchivoB64(lista, consulta);
                        //el 3er parametro es el numero de reporte que se esta marcando como consultado
                        reportes = _docMSv.MarcarConsultaRealizada(reportes, consulta, 1, archb64);
                        ArchivosCargadosModulo = reportes;

                        #endregion
                    }

                }

                metadata = MetadataGeneral;

                

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrilla(CuentaCorrienteBuscada, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

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
                _logger.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

       
        public async Task<IActionResult> ConsultarVencimiento(string ctaId, DateTime fechaD, DateTime fechaH)
        {
            List<ConsVtoDto> lista;
            GridCore<ConsVtoDto> grillaDatos;
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
                    throw new NegocioException(res.Mensaje);
                }
                VencimientosBuscados = res.ListaEntidad;

                if (res.ListaEntidad.Count > 0)
                {
                    #region Gestor Impresion - marcando que hay datos para el reporte n
                    var reportes = ArchivosCargadosModulo;
                    string archb64 = GenerarArchivoB64(res.ListaEntidad, consulta);
                    //el 3er parametro es el numero de reporte que se esta marcando como consultado
                    reportes = _docMSv.MarcarConsultaRealizada(reportes, consulta, 1, archb64);
                    ArchivosCargadosModulo = reportes;
                    #endregion
                }

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrilla(res.ListaEntidad, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

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
                _logger.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        public async Task<IActionResult> ConsultarCmpteTotal(string ctaId, int meses, bool relCuil)
        {
            List<ConsCompTotDto> lista;
            GridCore<ConsCompTotDto> grillaDatos;
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
                    throw new NegocioException(res.Mensaje);
                }
                CmptesTotalBuscados = res.ListaEntidad;

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrilla(res.ListaEntidad, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

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
                _logger.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        public async Task<IActionResult> ConsultarCmpteDetalle(string ctaId, string mes, bool relCuil)
        {
            List<ConsCompDetDto> lista;
            GridCore<ConsCompDetDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();

            var consulta = AppReportes.CCUENTAS_COMPROBANTES;

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
                    throw new NegocioException(res.Mensaje);
                }
                CmptesDetalleBuscados = res.ListaEntidad;

                if (res.ListaEntidad.Count > 0)
                {
                    #region Gestor Impresion - marcando que hay datos para el reporte n
                    var reportes = ArchivosCargadosModulo;
                    string archb64 = GenerarArchivoB64(res.ListaEntidad, consulta);
                    //el 3er parametro es el numero de reporte que se esta marcando como consultado
                    reportes = _docMSv.MarcarConsultaRealizada(reportes, consulta, 1, archb64);
                    ArchivosCargadosModulo = reportes;
                    #endregion
                }

                string provId = CuentaComercialSeleccionada.Prov_Id.ToString();
                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrilla(res.ListaEntidad, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
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
                _logger.LogError(ex, msg);
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
            List<ConsOrdPagosDto> lista;
            GridCore<ConsOrdPagosDto> grillaDatos;
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
                    throw new NegocioException(res.Mensaje);
                }
                OrdenPagosBuscados = res.ListaEntidad;
                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrilla(res.ListaEntidad, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

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
                _logger.LogError(ex, msg);
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
            List<ConsOrdPagosDetDto> lista;
            GridCore<ConsOrdPagosDetDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            var consulta = AppReportes.CCUENTAS_ORDEN_DE_PAGO;
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
                    throw new NegocioException(res.Mensaje);
                }
                OrdenPagosDetBuscados = res.ListaEntidad;

                if (res.ListaEntidad.Count > 0)
                {
                    #region Gestor Impresion - marcando que hay datos para el reporte n
                    var reportes = ArchivosCargadosModulo;
                    string archb64 = GenerarArchivoB64(res.ListaEntidad, consulta);
                    //el 3er parametro es el numero de reporte que se esta marcando como consultado
                    reportes = _docMSv.MarcarConsultaRealizada(reportes, consulta, 1, archb64);
                    ArchivosCargadosModulo = reportes;
                    #endregion
                }

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrilla(res.ListaEntidad, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

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
                _logger.LogError(ex, msg);
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
            List<ConsRecepcionProveedorDto> lista;
            GridCore<ConsRecepcionProveedorDto> grillaDatos;
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
                    throw new NegocioException(res.Mensaje);
                }
                RecepProvBuscados = res.ListaEntidad;
                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrilla(res.ListaEntidad, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

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
                _logger.LogError(ex, msg);
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
            List<ConsRecepcionProveedorDetalleDto> lista;
            GridCore<ConsRecepcionProveedorDetalleDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            var consulta = AppReportes.CCUENTAS_RECEPCION_PROVEEDORES;

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
                    throw new NegocioException(res.Mensaje);
                }
                RecepProvDetBuscados = res.ListaEntidad;

                if (res.ListaEntidad.Count > 0)
                {
                    #region Gestor Impresion - marcando que hay datos para el reporte n
                    var reportes = ArchivosCargadosModulo;
                    string archb64 = GenerarArchivoB64(res.ListaEntidad, consulta);
                    //el 3er parametro es el numero de reporte que se esta marcando como consultado (1=original). 
                    //se deberá ver cuando se tenga que imprimir un duplicado.deberia ser 2 o 3 o lo que corresponda.
                    reportes = _docMSv.MarcarConsultaRealizada(reportes, consulta, 1, archb64);
                    ArchivosCargadosModulo = reportes;
                    #endregion
                }

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrilla(res.ListaEntidad, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

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
                _logger.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        #region Metodos Privados
        private string GenerarArchivoB64<T>(List<T> lista, AppReportes consulta)
        {
            MemoryStream ms = new();
            string observacion = string.Empty;
            List<string> titulos;
            float[] anchos;
            switch (consulta)
            {
                case AppReportes.CCUENTAS_CUENTA_CORRIENTE:
                    observacion = "CUENTA CORRIENTE - Documento generado por el sistema de Gestión Comercial";
                    var cDatos = (lista as List<ConsCtaCteDto>).Select(x => new
                    {
                        Fecha = x.Cc_fecha.ToDateTime().ToShortDateString(),
                        DiaMovi = x.Dia_movi,
                        Comprobante = $"{x.Tco_desc}-{x.Cm_compte}",
                        Concepto = x.Cc_concepto,
                        Debe = x.Cc_debe,
                        Haber = x.Cc_haber,
                        Saldo = x.Cc_saldo
                    }).ToList();

                    titulos = new List<string> { "Fecha", "N° Mov", "Comprobante", "Concepto", "Debe", "Haber","Saldo" };
                    anchos = [10f, 10f, 20f, 30f, 10f, 10f, 10f];
                    GeneraReporteSegunDatos(consulta, cDatos, observacion, out ms, titulos, anchos);

                    break;
                case AppReportes.CCUENTAS_VENCIMIENTO_COMPROBANTES:
                    observacion = "VENCIMIENTOS DE COMPROBANTES - Documento generado por el sistema de Gestión Comercial";
                    var vDatos = (lista as List<ConsVtoDto>).Select(x => new
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

                    GeneraReporteSegunDatos(consulta, vDatos, observacion, out ms, titulos, anchos);
                    break;
                case AppReportes.CCUENTAS_COMPROBANTES:
                    break;
                case AppReportes.CCUENTAS_ORDEN_DE_PAGO:
                    break;
                case AppReportes.CCUENTAS_RECEPCION_PROVEEDORES:
                    break;
                case AppReportes.CCUENTAS_COMPROBANTES_DETALLE:
                    break;
                case AppReportes.CCUENTAS_ORDEN_DE_PAGO_DETALLE:
                    break;
                case AppReportes.CCUENTAS_RECEPCION_PROVEEDORES_DETALLE:
                    break;
                default:
                    break;
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        private void GeneraReporteSegunDatos<S>(AppReportes consulta, List<S> vDatos, string observacion, out MemoryStream ms, List<string> titulos, float[] anchos)
        {
            var cuenta = CuentaComercialSeleccionada;

            PrintRequestDto<S> request = new PrintRequestDto<S>();
            string logoPath = Path.Combine(_env.WebRootPath, "img", "gc.png");
            request.Cabecera = new DatosCabeceraDto
            {
                NombreEmpresa = _empresaGeco.Nombre,
                Direccion = _empresaGeco.Direccion,
                CUIT = _empresaGeco.CUIT,
                IIBB = _empresaGeco.IngresosBrutos,
                Sucursal = AdministracionId,
                TituloDocumento = $"{ModuloDM}_{cuenta.Cta_Id}_{DateTime.Now.Ticks}",
                Logo = logoPath,
            };
            request.Pie = new DatosPieDto
            {
                Usuario = UserName,
                Observaciones = observacion
            };
            request.Cuerpo = new DatosCuerpoDto<S>
            {
                CtaId = cuenta.Cta_Id,
                RazonSocial = cuenta.Cta_Denominacion,
                Domicilio = cuenta.Cta_Domicilio,
                CUIT = cuenta.Cta_Documento,
                Contacto = "-",
                Datos = vDatos,
            };

            _docMSv.GenerarArchivoPDF(request, out ms, titulos, anchos);
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
                return Lista.FirstOrDefault();
            }
            return new CuentaDto();
        }

        #endregion
    }
}
