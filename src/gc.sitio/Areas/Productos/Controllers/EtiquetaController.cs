using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Etiqueta;
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
           ILogger<EtiquetaController> logger, IOptions<DocsManager> docsManager,
           IDocManagerServicio docManagerServicio, IEtiquetaServicio etiquetaServicio,
           ICuentaServicio cuentaServicio,
            IRubroServicio rubroServicio) : base(options, contexo, logger)
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

        // Acción para construir y devolver el grid parcial con el detalle de etiquetas
        [HttpPost]
        public async Task<IActionResult> ObtenerDetalleEtiquetas([FromBody]  QueryFilters filters, int pag = 1)
        {
            try
            {
                if (filters is null)
                    return BadRequest("Parámetros inválidos.");

                filters.Adm_id = AdministracionId;
                filters.Usu_id = UserName;

                // Obtención optimizada (el servicio debe invocar el API). Ordenar en servidor si es posible.
                var resp = await _etSv.ObtenerDetalleEtiquetas(filters,TokenCookie);
                if (!resp.Ok)
                {
                    throw new NegocioException(resp.Mensaje ?? "Error al obtener detalle de etiquetas.");
                }

                // Ordenar por descripción para una UX consistente (evita ordenar en la vista)
                var ordenada = resp.ListaEntidad?.OrderBy(x => x.p_desc, StringComparer.OrdinalIgnoreCase).ToList();

                // GridCoreSmart centralizado desde la base
                var grid = GenerarGrillaSmart(ordenada, nameof(IEDetalleDto.p_desc));
                //grid.MetadataGeneral = MetadataGeneral; // mantener consistencia con el resto del sitio

                return PartialView("_EtiquetaDetalle", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener detalle de etiquetas.");
                return PartialView("_EtiquetaDetalle", GenerarGrillaSmart(new List<IEDetalleDto>(), nameof(IEDetalleDto.p_desc)));
            }
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
            ViewBag.Rel03B2 = HelperMvc<ComboGenDto>.ListaGenerica(listR03);

            var listaEtiqueta = new List<ComboGenDto>()
            {
                new ComboGenDto{Id="0", Descripcion="Puntera de Góndola"},
                new ComboGenDto{Id="1", Descripcion="Etiquetas 1 Precio, lista por defecto"},
                new ComboGenDto{Id="2", Descripcion="Etiquetas 2 Precios, lista por defecto y diferencial o segunda lista"},
            };
            ViewBag.TipoEtiqueta = HelperMvc<ComboGenDto>.ListaGenerica(listaEtiqueta);

            ObtenerCargaPrevia(AdministracionId, _etSv);

            var listCargaPrevia = new List<ComboGenDto>();
            ViewBag.CargaPrevia = ComboCargasPrevias();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarImpresionEtiqueta([FromBody] ConfirmarEtiquetaRequestDto request)
        {
            try
            {
                if (request is null)
                {
                    return BadRequest(new { ok = false, mensaje = "Los parámetros son obligatorios." });
                }

                // Asignar datos de contexto
                request.Adm = AdministracionId;
                request.Usu = UserName;

                // Invocar servicio
                var respuesta = await _etSv.ConfirmarImpresionEtiqueta(request, TokenCookie);

                if (!respuesta.Ok)
                {
                    return Ok(new { 
                        ok = false, 
                        mensaje = respuesta.Mensaje ?? "No se pudo confirmar la impresión de etiquetas." 
                    });
                }

                return Ok(new { 
                    ok = true, 
                    mensaje = "Impresión confirmada correctamente.",
                    data = respuesta.Entidad
                });
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error de negocio al confirmar impresión de etiquetas.");
                return Ok(new { ok = false, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inesperado al confirmar impresión de etiquetas.");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { ok = false, mensaje = "Ocurrió un error al procesar la solicitud." });
            }
        }
    }
}
