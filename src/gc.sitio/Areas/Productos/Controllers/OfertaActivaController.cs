using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;
using X.PagedList;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class OfertaActivaController : ControladorOfertaBase
    {
        // variables para manerjar modulo de impresión
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.OF_ACT.ToString();
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _configuracion;
        private readonly IOfertaServicio _ofertaServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IRubroServicio _rubroServicio;

        public OfertaActivaController(IOptions<AppSettings> options, IHttpContextAccessor contexo,
            ILogger<OfertasController> logger, IOfertaServicio ofertaServicio,
            ICuentaServicio cuenta, IRubroServicio rubro, IOptions<DocsManager> docsManager,
            IDocManagerServicio docManagerServicio) : base(options, contexo, logger)
        {
            _configuracion = options.Value;
            _ofertaServicio = ofertaServicio;
            _cuentaServicio = cuenta;
            _rubroServicio = rubro;
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManagerServicio;
        }

        public IActionResult Index()
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                string titulo = "Ofertas Activas";
                ViewData["Titulo"] = titulo;

                #region Gestor Impresion - Inicializacion de variables

                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
                ViewBag.ImpresionId = _modulo.Reportes[0].Id; //siempre el primer 

                _logger?.LogInformation($"Generando Arbol de Archivos del módulo. {MethodBase.GetCurrentMethod()?.Name}");

                //en este mismo acto se cargan los posibles documentos
                //que se pueden imprimir, exportar, enviar por email o whatsapp
                ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion

                OfertasActivas = [];
         
                return View();
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error de negocio al cargar la vista de Ofertas Activas");
                TempData["error"] = ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error de negocio al cargar la vista de Ofertas Activas");
                TempData["error"] = "Hubo un problema al cargar la vista de Ofertas Activas. Si el problema persiste, contacte al administrador.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> PresentarOfertasActivas(string admId = "0000", string lp_id = "001", int pag = 1)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                RespuestaGenerica<OfertaDto> respuesta = await _ofertaServicio.ObtenerOfertasActivas(admId, lp_id, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    var msg = respuesta.Mensaje ?? "Error al obtener ofertas activas";
                    TempData["error"] = msg;
                    throw new NegocioException(msg);
                }
                

                OfertasActivas = respuesta.ListaEntidad ?? [];

                var ofertas = OfertasActivas;
                int registrosPorPagina = _configuracion.NroRegistrosPagina;
                var pagedList = new StaticPagedList<OfertaDto>(
                    ofertas.OrderBy(o => o.p_desc).ToList(),
                    pag,
                    registrosPorPagina,
                    ofertas.Count
                );
                var grid = new GridCoreSmart<OfertaDto>
                {
                    ListaDatos = pagedList,
                    CantidadReg = ofertas.Count,
                    PrimerRegistro = ((pag - 1) * registrosPorPagina) + 1,
                    UltimoRegistro = Math.Min(pag * registrosPorPagina, ofertas.Count),
                    RegistroFinal = ofertas.Count,
                    CantidadPaginas = (int)Math.Ceiling((double)ofertas.Count / registrosPorPagina),
                    PaginaActual = pag,
                    Sort = "p_desc",
                    SortDir = "ASC",
                    DatoAux01 = $"Ofertas activas cargadas: {DateTime.Now:HH:mm:ss}"
                };
                return View("_gridOfertaActiva", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error interno al cargar ofertas activas");
                return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error interno al cargar ofertas activas");
                return PartialView("_gridMensaje", CrearRespuestaError("Error interno al cargar ofertas activas"));
            }
        }

        [HttpPost]
        public async Task<JsonResult> EliminarOfertasActivas(List<string> ids, string admId, string lp_id)
        {
            string msg = "Error interno al eliminar ofertas activas";
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { error = true, msg = "Sesión expirada" });


                if (ids == null || ids.Count == 0)
                {
                    return Json(new { error = true, msg = "Debe seleccionar al menos una oferta para eliminar." });
                }

                var idsSolicitados = ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
                if (idsSolicitados.Count == 0)
                    return Json(new { error = true, msg = "Debe seleccionar al menos una oferta para eliminar." });

                var estadoActual = await _ofertaServicio.ObtenerOfertasActivas(admId, lp_id, TokenCookie);
                if (!estadoActual.Ok || estadoActual.EsError)
                    return Json(new { error = true, msg = estadoActual.Mensaje ?? "No se pudo validar el estado actual de las ofertas." });

                var lista = estadoActual.ListaEntidad ?? [];
                var ofertasSeleccionadas = lista
                    .Where(o => idsSolicitados.Contains(o.p_id) && o.adm_id == admId && o.lp_id == lp_id)
                    .ToList();

                if (ofertasSeleccionadas.Count != idsSolicitados.Count)
                    return Json(new { error = true, msg = "La selección ya no coincide con el canal consultado. Actualice la grilla e intente nuevamente." });

                var ofertas = ofertasSeleccionadas.Select(p => new { p_id = p.p_id }).ToList();

                AbmPlusGenDto req = new AbmPlusGenDto
                {
                    Objeto = $"{admId}#{lp_id}",
                    Json = JsonConvert.SerializeObject(ofertas),
                    Usuario = UserName,
                    Administracion = AdministracionId
                };
                RespuestaGenerica<RespuestaDto> respuesta = await _ofertaServicio.EliminaOfertasActivas(req, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    throw new NegocioException(respuesta.Mensaje ?? "Error al eliminar las ofertas seleccionadas");
                }
                OfertasActivas = [];
                return Json(new
                {
                    error = false,
                    warn = false,
                    msg = idsSolicitados.Count == 1 ?
                            string.IsNullOrEmpty(respuesta.Mensaje) ? "Oferta eliminada correctamente." : respuesta.Mensaje :
                            string.IsNullOrEmpty(respuesta.Mensaje) ? "Ofertas eliminadas correctamente." : respuesta.Mensaje,
                    adm_Id = admId,
                    lp_id
                });

            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, msg);
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msg);
                return Json(new { error = true, warn = false, msg });
            }
        }

        /// <summary>
        /// Copia ofertas seleccionadas desde un canal origen a múltiples canales destino
        /// </summary>
        /// <param name="ids">Lista de IDs de los productos</param>
        /// <param name="admIdOrigen">ID de administración del canal origen</param>
        /// <param name="lpIdOrigen">ID de lista de precios del canal origen</param>
        /// <param name="canalesDestinoStr">Lista de canales destino en formato "admId#lpId"</param>
        /// <returns>Resultado de la operación de copia</returns>
        [HttpPost]
        public async Task<JsonResult> CopiarACanal(List<string> ids, string admIdOrigen, string lpIdOrigen, 
            List<string> canalesDestinoStr)
        {
            const string msgErrorBase = "Error interno al copiar ofertas a canales";
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { error = true, msg = "Sesión expirada" });

                // ✅ VALIDACIÓN: Parámetros requeridos
                if (ids == null || !ids.Any())
                    return Json(new { error = true, msg = "Debe seleccionar al menos una oferta para copiar" });

                if (string.IsNullOrEmpty(admIdOrigen) || string.IsNullOrEmpty(lpIdOrigen))
                    return Json(new { error = true, msg = "Debe especificar un canal origen válido" });

                if (canalesDestinoStr == null || !canalesDestinoStr.Any())
                    return Json(new { error = true, msg = "Debe seleccionar al menos un canal destino" });

                // Convertir los strings a tuplas (admId, lpId)
                var canalesDestino = canalesDestinoStr
                    .Select(canalStr => canalStr.Split('#'))
                    .Where(partes => partes.Length == 2 &&
                                      !string.IsNullOrWhiteSpace(partes[0]) &&
                                      !string.IsNullOrWhiteSpace(partes[1]))
                    .Select(partes => new { adm_id= partes[0], lp_id= partes[1]})
                    .Distinct()
                    .ToList();
                //var canalesDestino = new List<(string admId, string lpId)>();
                //foreach (var canalStr in canalesDestinoStr)
                //{
                //    var partes = canalStr.Split('#');
                //    if (partes.Length == 2)
                //    {
                //        canalesDestino.Add((partes[0], partes[1]));
                //    }
                //    else
                //    {
                //        _logger?.LogWarning($"Formato incorrecto de canal destino: {canalStr}");
                //    }
                //}

                if (!canalesDestino.Any())
                    return Json(new { error = true, msg = "No se pudo procesar ningún canal destino válido" });

                if (canalesDestino.Any(c => c.adm_id == admIdOrigen && c.lp_id == lpIdOrigen))
                    return Json(new { error = true, msg = "El canal origen no puede incluirse entre los canales destino" });

                var idsSolicitados = ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
                if (idsSolicitados.Count == 0)
                    return Json(new { error = true, msg = "Debe seleccionar al menos una oferta para copiar" });

                var estadoActual = await _ofertaServicio.ObtenerOfertasActivas(admIdOrigen, lpIdOrigen, TokenCookie);
                if (!estadoActual.Ok || estadoActual.EsError)
                    return Json(new { error = true, msg = estadoActual.Mensaje ?? "No se pudo validar el estado actual de las ofertas." });

                var ofertasSeleccionadas = (estadoActual.ListaEntidad ?? [])
                    .Where(o => idsSolicitados.Contains(o.p_id) && o.adm_id == admIdOrigen && o.lp_id == lpIdOrigen)
                    .Select(p => new { p_id = p.p_id })
                    .ToList();

                if (ofertasSeleccionadas.Count != idsSolicitados.Count)
                    return Json(new { error = true, msg = "La selección ya no coincide con el canal origen. Actualice la grilla e intente nuevamente." });

                // Crear el objeto de petición para el servicio
                var req = new AbmPlusGenDto
                {
                    // Canal origen
                    Objeto = $"{admIdOrigen}#{lpIdOrigen}",
                    
                    // Lista de productos/ofertas
                    Json = JsonConvert.SerializeObject(ofertasSeleccionadas),
                    
                    // Lista de canales destino
                    Json2 = JsonConvert.SerializeObject(canalesDestino),
                    
                    // Información del usuario
                    Usuario = UserName,
                    Administracion = AdministracionId
                };
                    
                // Llamar al servicio para copiar las ofertas
                var respuesta = await _ofertaServicio.CopiarACanal(req, TokenCookie);
                    
                if (!respuesta.Ok || respuesta.EsError)
                    return Json(new { 
                        error = true, 
                        msg = respuesta.Mensaje ?? "Error al copiar ofertas a los canales destino"
                    });
                    
                // Construir mensaje según cantidad de ofertas y canales
                string mensajeExito = $"{idsSolicitados.Count} oferta{(idsSolicitados.Count == 1 ? "" : "s")} " +
                                      $"copiada{(idsSolicitados.Count == 1 ? "" : "s")} a " +
                                      $"{canalesDestino.Count} canal{(canalesDestino.Count == 1 ? "" : "es")}";

                if (!string.IsNullOrEmpty(respuesta.Mensaje))
                    mensajeExito = respuesta.Mensaje;
                    
                // Devolver respuesta exitosa con información
                return Json(new {
                    error = false,
                    warn = respuesta.Entidad?.resultado > 0,
                    msg = mensajeExito,
                    admIdOrigen,
                    lpIdOrigen,
                    cantidadOfertas = idsSolicitados.Count,
                    cantidadCanales = canalesDestino.Count
                });
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, $"{msgErrorBase}: {ex.Message}");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msgErrorBase);
                return Json(new { error = true, warn = false, msg = msgErrorBase });
            }
        }
    }
}
