using AspNetCoreGeneratedDocument;
using AutoMapper.Execution;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace gc.sitio.Areas.Consultas.Controllers
{
    /// <summary>
    /// Corresponde el controlador a las CUENTAS COMERCIALES
    /// </summary>
    [Area("Consultas")]
    public class ConsCuentaController : ConsultaControladorBase
    {
        private readonly IOptions<AppSettings> options1;
        private readonly AppSettings _settings;
        private readonly ILogger<ConsCuentaController> _logger;
        private readonly IConsultasServicio _consSv;
        private readonly ICuentaServicio _cuentaServicio;
        public ConsCuentaController(IOptions<AppSettings> options, IHttpContextAccessor contexto,
            ILogger<ConsCuentaController> logger, IConsultasServicio consulta,
            ICuentaServicio cuentaServicio) :base(options,contexto,logger)
        {
            _consSv = consulta;
            options1 = options;
            _settings = options.Value;
            _logger = logger;
            _cuentaServicio = cuentaServicio;
        }

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
                ViewData["Titulo"] = "Consulta de Cuentas Comerciales";
                return View();
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Index", "home", new { area = "" });

            }
        }

        public async Task<IActionResult> ConsultarCuentaCorriente(string ctaId, DateTime fechaD, bool buscaNew, string sort = "p_desc", string sortDir = "asc",  int pag = 1)
        {
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

                    var res = await _consSv.ConsultarCuentaCorriente(ctaId, fechaD.Ticks, UserName, pag,registros, TokenCookie);
                    lista = res.Item1 ?? [];
                    MetadataGeneral = res.Item2 ?? new MetadataGrid();
                    CuentaCorrienteBuscada = lista;
                }

                metadata = MetadataGeneral;
                

                var cuenta =  await _cuentaServicio.ObtenerListaCuentaComercial(ctaId, 'T',TokenCookie);
                if(cuenta == null || cuenta.Count == 0)
                {
                    throw new NegocioException("No se encontraron los datos de la cuenta. Verifique conexión.");
                }

                CuentaComercialSeleccionada = cuenta.First();

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

        public async Task<IActionResult> ConsultarVencimiento(string ctaId, DateTime fechaD,DateTime fechaH)
        {
            List<ConsVtoDto> lista;
            GridCore<ConsVtoDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                //voy a invocar el servicio que me traiga los datos de la cuenta corriente 
                //para la cuenta seleccionada
                var res = await _consSv.ConsultaVencimientoComprobantesNoImputados(ctaId, fechaD.Ticks,fechaH.Ticks, UserName, TokenCookie);
                string sort = string.Empty;
                string sortDir = string.Empty;
                int pag = 1;


                if (!res.Ok)
                {
                    throw new NegocioException(res.Mensaje);
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

                string msg = $"Error en la invocación de la API - Consulta de Detalle de Comprobantes del Periodo {mes.Substring(4,2)}-{mes.Substring(0,4)}.";
                _logger.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
    }
}
