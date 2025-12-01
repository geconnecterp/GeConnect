using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Precio;
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
    public class PrecioListaController : ControladorOfertaBase
    {

        // variables para manerjar modulo de impresión
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.PRECIO_LISTA.ToString();
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _configuracion;

        private readonly IPrecioListaServicio _plSv;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IRubroServicio _rubroServicio;

        public PrecioListaController(IOptions<AppSettings> options, IHttpContextAccessor contexo,
          ILogger<PrecioListaController> logger, IOptions<DocsManager> docsManager,
          IDocManagerServicio docManagerServicio, IPrecioListaServicio servicio,
          ICuentaServicio cuentaServicio,
           IRubroServicio rubroServicio) : base(options, contexo, logger)
        {
            _configuracion = options.Value;

            // inicializo las variables para manejar el modulo de impresión
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManagerServicio;
            _plSv = servicio;
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

                string titulo = "Lista de Precios";
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

        [HttpPost]
        public async Task<IActionResult> ObtenerDetallePrecios([FromBody] QueryFilters filters)
        {
            // ✅ AGREGAR LOGGING PARA DEBUGGING
            _logger?.LogInformation("📥 ObtenerDetallePrecios - Inicio");
            _logger?.LogInformation("Filters recibidos: {@Filters}", filters);
            
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (filters is null)
                {
                    _logger?.LogWarning("⚠️ Filters es null");
                    return BadRequest("Parámetros inválidos.");
                }

                // ✅ LOGGING DE CAMPOS IMPORTANTES
                _logger?.LogInformation("Rel01 Count: {Count}", filters.Rel01?.Count ?? 0);
                _logger?.LogInformation("Rel02 Count: {Count}", filters.Rel02?.Count ?? 0);
                _logger?.LogInformation("Rel03 Count: {Count}", filters.Rel03?.Count ?? 0);
                _logger?.LogInformation("Rel04 Count: {Count}", filters.Rel04?.Count ?? 0);
                _logger?.LogInformation("FechaD: {FechaD}, FechaH: {FechaH}", filters.FechaD, filters.FechaH);
                _logger?.LogInformation("Opt1 (Incluir Costo): {Opt1}", filters.Opt1);

                filters.Adm_id = AdministracionId;
                filters.Usu_id = UserName;

                RespuestaGenerica<PrecioListaDetalleDto> resp = await _plSv.ObtenerDetallePrecios(filters, TokenCookie);
                
                if (!resp.Ok)
                {
                    _logger?.LogError("❌ Error en servicio: {Mensaje}", resp.Mensaje);
                    throw new NegocioException(resp.Mensaje ?? "Error al obtener detalle de precios.");
                }

                var ordenada = resp.ListaEntidad?.OrderBy(x => x.p_id, StringComparer.OrdinalIgnoreCase).ToList();
                
                _logger?.LogInformation("✅ Registros obtenidos: {Count}", ordenada?.Count ?? 0);

                var grid = GenerarGrillaSmart(ordenada, nameof(PrecioListaDetalleDto.p_desc));

                return PartialView("_plDetalle", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "💥 Error al obtener detalle de precios.");
                return PartialView("_plDetalle", GenerarGrillaSmart(new List<PrecioListaDetalleDto>(), nameof(PrecioListaDetalleDto.p_desc)));
            }
        }



        private void InicializaVista(bool actualizar = true)
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
            ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

            
            ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

           
            ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
            ViewBag.Rel03 = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

            ObtenerListaPrecios( _plSv);
            
            //para las distintas listas de precio
            ViewBag.Rel04List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
            ViewBag.Rel04 = ComboListaPrecios();
        }
    }
}
