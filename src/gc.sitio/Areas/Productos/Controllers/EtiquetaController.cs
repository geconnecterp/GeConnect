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
    public class EtiquetaController : ControladorOfertaBase
    {
        // variables para manerjar modulo de impresión
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.ETIQUETAS.ToString();
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _configuracion;
        
        private readonly IEtiquetaServicio _etSv;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IRubroServicio _rubroServicio;


        public EtiquetaController(IOptions<AppSettings> options, IHttpContextAccessor contexo,
           ILogger<OfertasController> logger, IOptions<DocsManager> docsManager,
           IDocManagerServicio docManagerServicio, IEtiquetaServicio etiquetaServicio,
           ICuentaServicio cuentaServicio,
            IRubroServicio rubroServicio) :base(options, contexo, logger)
        {
            _configuracion = options.Value;

            // inicializo las variables para manejar el modulo de impresión
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManagerServicio;
            _etSv = etiquetaServicio;
            _cuentaServicio = cuentaServicio;
            _rubroServicio = rubroServicio;
        }
        public IActionResult Index()
        {
            string msg = "Error de negocios al cargar la vista de PRESUPUESTOS";
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                string titulo = "Impresión de Etiquetas";
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
                InicializaVista();
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, msg);
                TempData["error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msg);
                TempData["error"] = "Hubo un problema al cargar la vista de ETIQUETAS. Si el problema persiste, contacte al administrador.";
            }

            return View();
        }

        private void InicializaVista(bool actualizar = false)
        {
            if (ProveedoresLista.Count == 0 || actualizar)
            {
                ObtenerProveedores(_cuentaServicio, "BI");
            }

            if (RubroLista.Count == 0 || actualizar)
            {
                ObtenerRubros(_rubroServicio);
            }

            var listR01 = new List<ComboGenDto>();
            ViewBag.Rel011List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

            var listR02 = new List<ComboGenDto>();
            ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

            var listR03 = new List<ComboGenDto>();
            ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);
            ViewBag.Rel03 = HelperMvc<ComboGenDto>.ListaGenerica(listR03);



            var listaEtiqueta = new List<ComboGenDto>()
            {
                new ComboGenDto{Id="1", Descripcion="Puntera de Góndola"},
                new ComboGenDto{Id="2", Descripcion="Etiquetas 1 Precio, lista por defecto"},
                new ComboGenDto{Id="3", Descripcion="Etiquetas 2 Precios, lista por defecto y diferencial o segunda lista"},
            };
            ViewBag.TipoEtiqueta = HelperMvc<ComboGenDto>.ListaGenerica(listaEtiqueta);

            ObtenerCargaPrevia(AdministracionId, _etSv);

            var listCargaPrevia = new List<ComboGenDto>();
            ViewBag.CargaPrevia = ComboCargasPrevias();
        }
    }
}
