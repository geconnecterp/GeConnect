using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Impositivo;
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
    public class ImpositivoController : ControladorOfertaBase
    {
        // variables para manerjar modulo de impresión
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.IMPOSITIVO.ToString();
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _configuracion;

        private readonly IProducto2Servicio _prod2Sv;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IRubroServicio _rubroServicio;

        public ImpositivoController(IOptions<AppSettings> options, IHttpContextAccessor contexo,
         ILogger<PrecioListaController> logger, IOptions<DocsManager> docsManager,
         IDocManagerServicio docManagerServicio, 
         ICuentaServicio cuentaServicio,
          IRubroServicio rubroServicio,
          IProducto2Servicio producto2) : base(options, contexo, logger)
        {
            _configuracion = options.Value;

            // inicializo las variables para manejar el modulo de impresión
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManagerServicio;
            _prod2Sv = producto2;
            _cuentaServicio = cuentaServicio;
            _rubroServicio = rubroServicio;
        }

        public IActionResult Index()
        {
            string msg = "Error de negocios al cargar la vista con Dato Impositivo";
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
                TempData["error"] = "Hubo un problema al cargar la vista de Datos Impositivos. Si el problema persiste, contacte al administrador.";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerDatoImpositivo([FromBody] QueryFilters filters)
        {
            // ✅ AGREGAR LOGGING PARA DEBUGGING
            _logger?.LogInformation("📥 ObtenerDatoImpositivo - Inicio");
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

                filters.Adm_id = AdministracionId;
                filters.Usu_id = UserName;

                RespuestaGenerica<ImpositivoDatoDto> resp = await _prod2Sv.ObtenerDatoImpositivo(filters, TokenCookie);

                if (!resp.Ok)
                {
                    _logger?.LogError("❌ Error en servicio: {Mensaje}", resp.Mensaje);
                    throw new NegocioException(resp.Mensaje ?? "Error al obtener el Dato Impositivo.");
                }

                var ordenada = resp.ListaEntidad?.OrderBy(x => x.rubg_id).ThenBy(x => x.rub_id).ThenBy(x => x.cta_id).ToList();

                _logger?.LogInformation("✅ Registros obtenidos: {Count}", ordenada?.Count ?? 0);

                var grid = GenerarGrillaSmart(ordenada, nameof(ImpositivoDatoDto.p_desc));

                return PartialView("_datosImpDetalle", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "💥 Error al obtener el Dato Impositivo.");
                return PartialView("_datosImpDetalle", GenerarGrillaSmart(new List<ImpositivoDatoDto>(), nameof(PrecioListaDetalleDto.p_desc)));
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

            var rubs = RubroLista
                .Select(r => new ComboGenDto
                {
                    Id = r.Rub_Id,
                    Descripcion = r.Rub_Id + " - " + r.Rub_Desc
                })
                .ToList();
            ViewBag.Rel02 = HelperMvc<ComboGenDto>.ListaGenerica(rubs);

            var listR01 = new List<ComboGenDto>();
            ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);


            ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);


            ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
            ViewBag.Rel03 = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

            var condIva = ComboIVASituacion(_prod2Sv).GetAwaiter().GetResult();
            var alicIva = ComboIVAAlicuota(_prod2Sv).GetAwaiter().GetResult();
            //datos impositivos
            ViewBag.CondicionIva = condIva;
            ViewBag.AlicuotaIva = alicIva;
        }
    }
}
