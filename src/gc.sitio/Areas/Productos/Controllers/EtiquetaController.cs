using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using gc.infraestructura.Dtos.Productos.Ofertas;
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
        private readonly IOfertaServicio _ofertaServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IRubroServicio _rubroServicio;

        public EtiquetaController(IOptions<AppSettings> options, IHttpContextAccessor contexo,
           ILogger<EtiquetaController> logger, IOptions<DocsManager> docsManager,
            IDocManagerServicio docManagerServicio, IEtiquetaServicio etiquetaServicio,
            ICuentaServicio cuentaServicio,
             IRubroServicio rubroServicio, IOfertaServicio ofertaServicio) : base(options, contexo, logger)
        {
            _configuracion = options.Value;

            // inicializo las variables para manejar el modulo de impresión
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManagerServicio;
            _etSv = etiquetaServicio;
            _cuentaServicio = cuentaServicio;
            _rubroServicio = rubroServicio;
            _ofertaServicio = ofertaServicio;
        }

        public async Task<IActionResult> Index()
        {
            string msg = "Error de negocios al cargar la vista de ETIQUETAS";
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                LimpiarEstadoEtiqueta();

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
                await InicializaVista();
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
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (filters is null)
                    return BadRequest(new { ok = false, mensaje = "Los parámetros de búsqueda son obligatorios." });

                var tieneFechaDesde = filters.FechaD.HasValue && filters.FechaD.Value != DateTime.MinValue;
                var tieneFechaHasta = filters.FechaH.HasValue && filters.FechaH.Value != DateTime.MinValue;

                if (tieneFechaDesde != tieneFechaHasta)
                {
                    return BadRequest(new { ok = false, mensaje = "Debe indicar las fechas desde y hasta." });
                }

                if (tieneFechaDesde && filters.FechaD!.Value.Date > filters.FechaH!.Value.Date)
                {
                    return BadRequest(new { ok = false, mensaje = "La fecha desde no puede ser posterior a la fecha hasta." });
                }

                filters.Adm_id = AdministracionId;
                filters.Usu_id = UserName;
                if (filters.Opt2 != true)
                {
                    filters.OfertaList = [];
                }

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
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error de negocio al obtener detalle de etiquetas.");
                return BadRequest(new
                {
                    ok = false,
                    mensaje = string.IsNullOrWhiteSpace(ex.Message)
                        ? "No se pudo obtener el detalle de etiquetas."
                        : ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener detalle de etiquetas.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { ok = false, mensaje = "No se pudo obtener el detalle de etiquetas. Revise los filtros e intente nuevamente." });
            }
        }

        [HttpPost]
        public JsonResult BuscarProveedores(string prefix)
        {
            var texto = prefix?.Trim() ?? string.Empty;
            if (texto.Length < 3)
            {
                return Json(Array.Empty<ProveedorAutocompleteDto>());
            }

            var proveedores = ProveedoresLista
                .Where(x => !string.IsNullOrWhiteSpace(x.Cta_Lista) &&
                            x.Cta_Lista.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .Select(x => new ProveedorAutocompleteDto
                {
                    Id = x.Cta_Id,
                    Descripcion = $"{x.Cta_Lista}#{x.Tipo_Desc}",
                    TipoDesc = x.Tipo_Desc
                })
                .ToList();

            return Json(proveedores);
        }

        [HttpPost]
        public IActionResult ReiniciarEstado()
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
                return redirectResult;

            LimpiarEstadoEtiqueta();
            return Ok(new { ok = true });
        }

        private async Task InicializaVista(bool actualizar = false)
        {
            if (ProveedoresLista.Count == 0 || actualizar)
            {
                ObtenerProveedores(_cuentaServicio, "BI");
            }

            if (RubroLista.Count == 0 || actualizar)
            {
                ObtenerRubros(_rubroServicio);
            }

            #region Carga de Rubros
            var rubs = RubroLista
                .Select(r => new ComboGenDto
                {
                    Id = r.Rub_Id,
                    Descripcion = r.Rub_Id + " - " + r.Rub_Desc
                })
                .ToList();
            ViewBag.Rel02B2 = HelperMvc<ComboGenDto>.ListaGenerica(rubs);
            ViewBag.Rel02 = HelperMvc<ComboGenDto>.ListaGenerica(rubs);

            #endregion


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

            var tiposOferta = await _ofertaServicio.BuscarTiposOferta(TokenCookie);
            if (!tiposOferta.Ok || tiposOferta.EsError)
            {
                throw new NegocioException(tiposOferta.Mensaje ?? "No se pudieron obtener los tipos de oferta.");
            }

            ViewBag.TiposOfertaFiltro = tiposOferta.ListaEntidad ?? new List<TipoOfertaDto>();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarImpresionEtiqueta([FromBody] ConfirmarEtiquetaRequestDto request)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

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

                LimpiarEstadoEtiqueta();

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

        private void LimpiarEstadoEtiqueta()
        {
            HttpContext.Session.Remove("CargasPrevias");
            HttpContext.Session.Remove("DocumentManager");
            HttpContext.Session.Remove("ArchivosCargadosModulo");
        }
    }
}
