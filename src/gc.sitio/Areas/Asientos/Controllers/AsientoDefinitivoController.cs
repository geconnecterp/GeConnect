using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.core.Servicios.Contratos.ABM;
using gc.sitio.core.Servicios.Contratos.Asientos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Asientos.Controllers
{
    [Area("Asientos")]
    public class AsientoDefinitivoController : AsientoBase
    {
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.ASTEMP.ToString();

        private readonly IAsientoFrontServicio _asientoServicio;
        private readonly IAsientoDefinitivoServicio _asDefSv;
        private readonly IAbmServicio _abmSv;
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _appSettings;   

        public AsientoDefinitivoController(
            IOptions<AppSettings> options, IOptions<DocsManager> docsManager,
            IHttpContextAccessor contexto,
            ILogger<AsientoDefinitivoController> logger,
            IAsientoFrontServicio asientoServicio,
            IAsientoDefinitivoServicio asDefSv,
            IAbmServicio abm,
            IDocManagerServicio docManager
            ) : base(options, contexto, logger)
        {
            _asientoServicio = asientoServicio;
            _asDefSv = asDefSv;
            _appSettings = options.Value;
            _abmSv = abm;
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {


                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                string titulo = "Asientos DEFINITIVOS";
                ViewData["Titulo"] = titulo;

                #region Gestor Impresion - Inicializacion de variables
                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
                // en este mismo acto se cargan los posibles documentos
                //que se pueden imprimir, exportar, enviar por email o whatsapp
                ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion

                // Obtenemos los datos para los combos
                await ObtenerEjerciciosContables(_asientoServicio);
                ViewBag.TiposAsientoLista = new List<TipoAsientoDto>();
                UsuariosEjercicioLista = new List<UsuAsientoDto>();

                // Asignamos los combos a la vista
                ViewBag.ListaEjercicios = ComboEjercicios();
                ViewBag.ListaTiposAsiento = ComboTiposAsiento();
                ViewBag.ListaUsuarios = ComboUsuariosEjercicio();

                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cargar la vista de Asientos Definitivos");
                TempData["error"] = "Hubo un problema al cargar la vista de Asientos Definitivos. Si el problema persiste, contacte al administrador.";
                return View();
            }
        }


    }
}
