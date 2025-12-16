using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Reflection;
using X.PagedList;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class ProveedorModiController : ControladorOfertaBase
    {

        // variables para manerjar modulo de impresión
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.PROVEEDOR_SIN_MODIFICACIONES.ToString();

        private readonly IDocManagerServicio _docMSv;
        private readonly AppSettings _configuracion;
        private readonly IProducto2Servicio _prodSv;

        public ProveedorModiController(IOptions<AppSettings> options,
            IHttpContextAccessor contexo,
            ILogger<OfertasController> logger,
            IOptions<DocsManager> docsManager,
             IDocManagerServicio docManagerServicio,
            IProducto2Servicio prodSv) : base(options, contexo, logger)
        {
            _prodSv = prodSv;
            _configuracion = options.Value;

            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManagerServicio;
        }

        public IActionResult Index()
        {
            string msg = "Error al inicializar el Módulo de Proveedor sin Modificaciones de Precio.";

            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;
                string titulo = "Proveedor sin Modificación de Precios";
                ViewData["Titulo"] = titulo;

                #region Gestor Impresion - Inicializacion de variables

                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
                ViewBag.ImpresionId = _modulo.Reportes[0].Id; //siempre el primer reporte

                _logger?.LogInformation($"Generando Arbol de Archivos del módulo. {MethodBase.GetCurrentMethod()?.Name}");

                //en este mismo acto se cargan los posibles documentos
                //que se pueden imprimir, exportar, enviar por email o whatsapp
                ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion


            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, msg);
                TempData["error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msg);
                TempData["error"] = msg;
            }
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> ObtenerProveedoresSinModificacionPr([FromBody]DateTime desde)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                RespuestaGenerica<ProvSinModPrecioDto> respuesta = await _prodSv.ProvSinModPrecio(desde, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    var msg = respuesta.Mensaje ?? "Error al obtener los datos consultados";
                    TempData["error"] = msg;
                    throw new NegocioException(msg);
                }

                var lista = respuesta.ListaEntidad ?? [];
                int registrosPorPagina = _configuracion.NroRegistrosPagina;
                var pagedList = new StaticPagedList<ProvSinModPrecioDto>(
                    lista.OrderBy(o => o.cta_denominacion).ToList(),
                    1,
                    registrosPorPagina,
                    lista.Count
                );
                var grid = new GridCoreSmart<ProvSinModPrecioDto>
                {
                    ListaDatos = pagedList,
                    CantidadReg = lista.Count,
                    PrimerRegistro = ((1 - 1) * registrosPorPagina) + 1,
                    UltimoRegistro = Math.Min(1 * registrosPorPagina, lista.Count),
                    RegistroFinal = lista.Count,
                    CantidadPaginas = (int)Math.Ceiling((double)lista.Count / registrosPorPagina),
                    PaginaActual = 1,
                    Sort = "cta_denominacion",
                    SortDir = "ASC",
                    DatoAux01 = $"Cargado: {DateTime.Now:HH:mm:ss}"
                };
                return View("_gridProvSinModi", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error interno al cargar ofertas sin activar");
                return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error interno al cargar ofertas sin activar");
                return PartialView("_gridMensaje", CrearRespuestaError("Error interno al cargar ofertas sin activar"));
            }
        }
    }
}
