using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class OfertaReportController : ControladorOfertaBase
    {
        // variables para manerjar modulo de impresión
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.OF_REPORTE.ToString();
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _configuracion;
        private readonly IOfertaServicio _ofertaServicio;

        public OfertaReportController(IOptions<AppSettings> options, 
            IHttpContextAccessor contexo,
            ILogger<OfertasController> logger,
            IOfertaServicio ofertaServicio,
            IComboServicio cmbSv,
            IOptions<DocsManager> docsManager,
            IDocManagerServicio docManagerServicio
            )
            : base(options, contexo, logger)
        {
            _configuracion = options.Value;
            _ofertaServicio = ofertaServicio;
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManagerServicio;
            _ofertaServicio = ofertaServicio;
        }

        public IActionResult Index()
        {
            string msg = "Error al inicializar el Módulo de Reporte de Ofertas y Combos.";

            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;
                string titulo = "REPORTE DE OFERTAS Y COMBOS";
                ViewData["Titulo"] = titulo;

                #region Gestor Impresion - Inicializacion de variables

                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
                ViewBag.ImpresionId = _modulo.Reportes.Select(x => x.Id).ToArray(); //siempre el primer reporte

                _logger?.LogInformation($"Generando Arbol de Archivos del módulo. {MethodBase.GetCurrentMethod()?.Name}");

                //en este mismo acto se cargan los posibles documentos
                //que se pueden imprimir, exportar, enviar por email o whatsapp
                ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion

                InicializarVista();

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

        private void InicializarVista()
        {
            var canales = _ofertaServicio.BuscarCanales(TokenCookie).GetAwaiter().GetResult();
            var cmb = canales.ListaEntidad?.Select(x => new ComboGenDto { Id = x.lp_id, Descripcion = x.canal }).ToList();
            ViewBag.Tipo = HelperMvc<ComboGenDto>.ListaGenerica(cmb ?? []);
        }
    }
}
