using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Dtos.Inventario.Dto;
using gc.infraestructura.Dtos.Inventario.Request;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Implementacion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using X.PagedList;

namespace gc.pocket.site.Areas.Gestion.Controllers
{
    [Area("Gestion")]
    public class InventarioController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly AppSettings _configuracion;
        private readonly ILogger<AlmacenController> _logger;
        private readonly IInventarioServicio _invSv;

        public InventarioController(ILogger<AlmacenController> logger,
            IOptions<MenuSettings> options,
            IOptions<AppSettings> options2,
            IInventarioServicio invSv,
            IOptions<AppSettings> options1, IHttpContextAccessor context) : base(options1, options, context, logger)
        {
            _logger = logger;
            _menuSettings = options.Value;
            _invSv = invSv;
            _configuracion = options2.Value;
        }
        public IActionResult Index()
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
                return redirectResult;

            var sigla = "inv";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }

        [HttpPost]
        public IActionResult ObtenerInventarioLista()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;
                GetInventarioListaRequest req = new GetInventarioListaRequest
                {
                    desde = new(2020, 1, 1),
                    hasta = DateTime.Now,
                    adm_id = AdministracionId,
                    usu_id = "%",//UserName,
                    inve_id = "S"
                };
                var respuesta = _invSv.GetInventarioLista(req, TokenCookie);
                if (respuesta == null)
                {
                    var msg = "Error al obtener los inventarios";
                    TempData["error"] = msg;
                    throw new NegocioException(msg);
                }


                var lista = respuesta;
                int registrosPorPagina = _configuracion.NroRegistrosPagina;
                var pagedList = new StaticPagedList<InventarioListaDto>(
                    lista.OrderBy(o => o.inv_nro).ToList(),
                    1,
                    registrosPorPagina,
                    lista.Count
                );
                var grid = new GridCoreSmart<InventarioListaDto>
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
                return PartialView("_gridInventarios", grid);
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

        [HttpPost]
        public async Task<IActionResult> ObtenerInventarioBox([FromBody] string inv_nro)
        {
            int pag = 1;
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;
                var req = new InventarioRequestDto
                {
                    inv_nro = inv_nro,
                    usu_id = UserName
                };
                RespuestaGenerica<InventarioBoxDto> respuesta = await _invSv.GetInventarioBox(req, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    var msg = respuesta.Mensaje ?? "Error al obtener ofertas sin activar";
                    TempData["error"] = msg;
                    throw new NegocioException(msg);
                }


                var box = respuesta.ListaEntidad ?? []; ;
                int registrosPorPagina = _configuracion.NroRegistrosPagina;
                var pagedList = new StaticPagedList<InventarioBoxDto>(
                    box.OrderBy(o => o.box_id).ToList(),
                    pag,
                    registrosPorPagina,
                    box.Count
                );
                var grid = new GridCoreSmart<InventarioBoxDto>
                {
                    ListaDatos = pagedList,
                    CantidadReg = box.Count,
                    PrimerRegistro = ((pag - 1) * registrosPorPagina) + 1,
                    UltimoRegistro = Math.Min(pag * registrosPorPagina, box.Count),
                    RegistroFinal = box.Count,
                    CantidadPaginas = (int)Math.Ceiling((double)box.Count / registrosPorPagina),
                    PaginaActual = pag,
                    Sort = "box_id",
                    SortDir = "ASC",
                    DatoAux01 = $"Box cargados: {DateTime.Now:HH:mm:ss}"
                };
                return View("_gridInventarioBox", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error interno al cargar los Box");
                return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error interno al cargar los Box");
                return PartialView("_gridMensaje", CrearRespuestaError("Error interno al cargar los Box"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerInventarioPlanilla([FromBody] string inv_nro)
        {
            int pag = 1;
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                var req = new InventarioRequestDto
                {
                    inv_nro = inv_nro,
                    usu_id = UserName
                };

                RespuestaGenerica<InventarioPlanillaDto> respuesta = await _invSv.GetInventarioPlanilla(req, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    var msg = respuesta.Mensaje ?? "Error al obtener ofertas sin activar";
                    TempData["error"] = msg;
                    throw new NegocioException(msg);
                }


                var box = respuesta.ListaEntidad ?? []; ;
                int registrosPorPagina = _configuracion.NroRegistrosPagina;
                var pagedList = new StaticPagedList<InventarioPlanillaDto>(
                    box.OrderBy(o => o.inv_nro).ToList(),
                    pag,
                    registrosPorPagina,
                    box.Count
                );
                var grid = new GridCoreSmart<InventarioPlanillaDto>
                {
                    ListaDatos = pagedList,
                    CantidadReg = box.Count,
                    PrimerRegistro = ((pag - 1) * registrosPorPagina) + 1,
                    UltimoRegistro = Math.Min(pag * registrosPorPagina, box.Count),
                    RegistroFinal = box.Count,
                    CantidadPaginas = (int)Math.Ceiling((double)box.Count / registrosPorPagina),
                    PaginaActual = pag,
                    Sort = "inv_nro",
                    SortDir = "ASC",
                    DatoAux01 = $"Planillas cargadas: {DateTime.Now:HH:mm:ss}"
                };
                return View("_gridInventarioPlantilla", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error interno al cargar las planillas");
                return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error interno al cargar las planillas");
                return PartialView("_gridMensaje", CrearRespuestaError("Error interno al cargar los Box"));
            }
        }

        [HttpPost]
        public async Task<JsonResult> ValidarConteo([FromBody]InventarioRequestDto req)
        {
            try
            {
                // Verificar autenticación
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                if (req == null)
                {
                    throw new NegocioException("Los datos del conteo son incorrectos");
                }

                if (string.IsNullOrEmpty(req.tipo_id))
                {
                    if (req.tipo.Equals('B'))
                    {
                        throw new NegocioException("Es necesario que ingrese algun BOX para poder proceder");
                    }
                    else
                    {
                        throw new NegocioException("Es necesario que seleccione alguna Planilla antes de proceder.");
                    }
                }

                req.usu_id = UserName;

                RespuestaGenerica<RespuestaDto> resultado = await _invSv.ValidaConteo(req, TokenCookie);
                if(resultado == null || !resultado.Ok)
                {
                    if(resultado==null)
                    {
                        throw new NegocioException("Error al validar el conteo");
                    }

                    if (resultado.EsWarn)
                    {
                        throw new NegocioException(resultado.Mensaje ?? "Error al validar el conteo");  
                    }
                    if (resultado.EsError)
                    {
                        throw new Exception(resultado.Mensaje ?? "Error al validar el conteo");
                    }
                }
                return Json(new { error = false, warn = false, msg = "Validación Exitosa." });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ObtenerConteo([FromBody] InventarioRequestDto req)
        {
            try
            {
                // Verificar autenticación
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                if (req == null)
                {
                    throw new NegocioException("Los datos del conteo son incorrectos");
                }

                if (string.IsNullOrEmpty(req.tipo_id))
                {
                    if (req.tipo.Equals('B'))
                    {
                        throw new NegocioException("Es necesario que ingrese algun BOX para poder proceder");
                    }
                    else
                    {
                        throw new NegocioException("Es necesario que seleccione alguna Planilla antes de proceder.");
                    }
                }

                req.usu_id = UserName;

                var resultado = await _invSv.GetConteno(req, TokenCookie);
                if (resultado == null || !resultado.Ok)
                {
                    if (resultado == null)
                    {
                        throw new NegocioException("Error al obtener el conteo");
                    }

                    if (resultado.EsWarn)
                    {
                        throw new NegocioException(resultado.Mensaje ?? "Error al obtener el conteo");
                    }
                    if (resultado.EsError)
                    {
                        throw new Exception(resultado.Mensaje ?? "Error al obtener el conteo");
                    }
                }
                return Json(new { error = false, warn = false, msg = "Validación Exitosa." });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CargaConteo(string invNro,string tipo,string tipoId)
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
                return redirectResult;

            var sigla = "inv";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            string? volver = Url.Action("index", "inventario", new { area = "gestion" });
            ViewBag.AppItem = new AppItem { Nombre = modulo.Nombre, VolverUrl = volver ?? "#" };

            if (string.IsNullOrEmpty(invNro) || 
                string.IsNullOrEmpty(tipo) || 
                string.IsNullOrEmpty(tipoId))
            {
                TempData["error"] = "Los datos del conteo son incorrectos";
                return RedirectToAction("Index");
            }
            var req = new InventarioRequestDto { inv_nro = invNro, tipo = tipo[0], tipo_id = tipoId };
            var resultado = await _invSv.GetConteno(req, TokenCookie);
            if (resultado == null || !resultado.Ok)
            {
                if (resultado == null)
                {
                    TempData["error"] = "Error al obtener el conteo";
                    return RedirectToAction("index", "inventario", new { area = "Gestion" });
                }

                if (resultado.EsWarn)
                {
                    TempData["warn"] = resultado.Mensaje ?? "Error al obtener el conteo";
                    return RedirectToAction("index", "inventario", new { area = "Gestion" });
                }
                if (resultado.EsError)
                {
                    TempData["error"] =resultado.Mensaje ?? "Error al obtener el conteo";
                    return RedirectToAction("index", "inventario", new { area = "Gestion" });
                }
            }
            //return Json(new { error = false, warn = false, msg = "Validación Exitosa." });

            ViewBag.InvNro = invNro;
            ViewBag.Tipo = tipo;
            ViewBag.TipoId = tipoId;
            ViewBag.RegistrosConteo = resultado.ListaEntidad;
            return View(modulo);
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarConteo([FromBody]InventarioRequestDto req)
        {
            try
            {
                // Verificar autenticación
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                if (req == null)
                {
                    throw new NegocioException("Los datos para confirmar el conteo son incorrectos");
                }

                if (string.IsNullOrEmpty(req.tipo_id))
                {
                    if (req.tipo.Equals('B'))
                    {
                        throw new NegocioException("Es necesario que ingrese algun BOX para poder proceder");
                    }
                    else
                    {
                        throw new NegocioException("Es necesario que seleccione alguna Planilla antes de proceder.");
                    }
                }

                if (req.json.Count == 0)
                {
                    throw new NegocioException("Es necesario que al menos un producto sea enviado para confirmar.");
                }

                req.usu_id = UserName;
                req.json_p = JsonConvert.SerializeObject(req.json);

                RespuestaGenerica<RespuestaDto> resultado = await _invSv.ConfirmarConteo(req, TokenCookie);
                if (resultado == null || !resultado.Ok)
                {
                    if (resultado == null)
                    {
                        throw new NegocioException("Error al validar el conteo");
                    }

                    if (resultado.EsWarn)
                    {
                        throw new NegocioException(resultado.Mensaje ?? "Error al validar el conteo");
                    }
                    if (resultado.EsError)
                    {
                        throw new Exception(resultado.Mensaje ?? "Error al validar el conteo");
                    }
                }
                return Json(new { error = false, warn = false, msg = "Validación Exitosa." });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }
    }
}
