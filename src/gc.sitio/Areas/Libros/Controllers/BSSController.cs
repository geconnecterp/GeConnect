using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.Asientos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using gc.sitio.core.Servicios.Contratos.Libros;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Libros.Controllers
{
    [Area("Libros")]
    public class BSSController : MayorBase
    {
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.BSS.ToString();
        private readonly AppSettings _appSettings;

        private readonly IAsientoFrontServicio _asientoServicio;
        private readonly IBSSServicio _bssServicio;
        private readonly IDocManagerServicio _docMSv;
        public BSSController(
            IOptions<AppSettings> options,
            IOptions<DocsManager> docsManager,
            IHttpContextAccessor contexto,
            ILogger<BSSController> logger,
            IDocManagerServicio docManager,
            IBSSServicio bSSServicio,
            IAsientoFrontServicio asientoFront) : base(options, contexto, logger)
        {
            _appSettings = options.Value;
            _docsManager = docsManager.Value;
            _asientoServicio = asientoFront;
            _bssServicio = bSSServicio;
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

                string titulo = "Balance de Sumas y Saldos";
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

                // Asignamos los combos a la vista
                ViewBag.ListaEjercicios = ComboEjercicios();

                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cargar la vista de BSS");
                TempData["error"] = "Hubo un problema al cargar la vista del BSS. Si el problema persiste, contacte al administrador.";
                return View();
            }
        }

        /// <summary>
        /// Obtiene la lista de ejercicios contables para cargar el combo en la vista
        /// </summary>
        /// <returns>Lista de ejercicios en formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> ObtenerEjercicios()
        {
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Obtener token de autenticación
                string token = TokenCookie;
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { error = true, msg = "No se encontró un token de autenticación válido." });
                }

                // Llamar al servicio para obtener los ejercicios
                var response = await _asientoServicio.ObtenerEjercicios(token);

                // Validar si hay error en la respuesta
                if (response.EsError)
                {
                    return Json(new { error = true, msg = response.Mensaje });
                }

                // Obtener la lista de ejercicios contables
                var ejercicios = response.ListaEntidad ?? new List<EjercicioDto>();

                // Preparar datos para la respuesta
                var resultado = ejercicios.OrderByDescending(x => x.Eje_ctl).Select(e => new
                {
                    eje_nro = e.Eje_nro,
                    eje_lista = e.Eje_lista
                }).ToList();

                return Json(resultado);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener ejercicios contables");
                return Json(new { error = true, msg = "Error al obtener ejercicios contables: " + ex.Message });
            }
        }
    }
}
