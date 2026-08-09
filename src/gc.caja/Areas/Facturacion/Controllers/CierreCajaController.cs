using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class CierreCajaController : ControladorBaseCaja
    {
        private readonly ICajaInitServicio _cajaInitServicio;
        private readonly IRendicionServicio _rendicionServicio;
        private readonly IFactDiferidaServicio _factDiferidaServicio;
        private readonly ICajaServicio _cajaServicio;

        public CierreCajaController(
            IOptions<AppSettings> options,
            ILogger<CierreCajaController> logger,
            IHttpContextAccessor httpContext,
            ICajaInitServicio cajaInitServicio,
            IRendicionServicio rendicionServicio,
            IFactDiferidaServicio factDiferidaServicio,
            ICajaServicio cajaServicio)
            : base(options, httpContext, logger)
        {
            _cajaInitServicio = cajaInitServicio;
            _rendicionServicio = rendicionServicio;
            _factDiferidaServicio = factDiferidaServicio;
            _cajaServicio = cajaServicio;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                return redirectResult;
            }

            var (cajaOk, mensajeCaja, caja) = await AsegurarCajaHidratadaParaCierre();
            if (!cajaOk)
            {
                TempData["Error"] = mensajeCaja;
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }

            ViewBag.Usuario = UserName;
            ViewBag.CajaId = caja.CajaId;
            ViewBag.CajaNombre = caja.Caja.caja_nombre ?? string.Empty;
            ViewBag.Proceso = caja.Caja.caja_nro_proceso ?? string.Empty;
            ViewBag.Cierre = caja.Caja.caja_nro_cierre ?? string.Empty;

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ValidacionInicial()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { success = false, message = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                var (cajaOk, mensajeCaja, caja) = await AsegurarCajaHidratadaParaCierre();
                if (!cajaOk)
                {
                    return Json(new { success = false, message = mensajeCaja });
                }

                var (esValido, mensajeValidacion) = _cajaInitServicio.ValidarDatosIniciales(caja);
                if (!esValido)
                {
                    _logger?.LogWarning(
                        "Cierre caja: validacion inicial fallida. CajaId={CajaId}; Usuario={Usuario}; Motivo={Motivo}",
                        caja.CajaId,
                        UserName,
                        mensajeValidacion);

                    return Json(new { success = false, message = mensajeValidacion });
                }

                _logger?.LogInformation(
                    "Cierre caja: validacion inicial exitosa. CajaId={CajaId}; Usuario={Usuario}",
                    caja.CajaId,
                    UserName);

                return Json(new
                {
                    success = true,
                    message = "Caja validada correctamente.",
                    caja_id = caja.CajaId,
                    caja_nombre = caja.Caja.caja_nombre ?? string.Empty,
                    usuario = UserName
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cierre caja: error inesperado en ValidacionInicial. Usuario={Usuario}", UserName);
                return Json(new { success = false, message = "Error interno al validar los datos de caja." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> VerificarPendientes()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                var resultado = await ObtenerPendientesDelCierreActual();
                if (!resultado.Ok)
                {
                    return Json(new { ok = false, mensaje = resultado.Mensaje });
                }

                return Json(new
                {
                    ok = true,
                    hayPendientes = resultado.Pendientes.Count > 0,
                    mensaje = resultado.Pendientes.Count > 0
                        ? "Existen cobranzas diferidas pendientes para el cierre actual."
                        : "No se encontraron facturas pendientes de clientes.",
                    lista = resultado.Pendientes
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cierre caja: error verificando pendientes. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudieron verificar las cobranzas diferidas pendientes." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CargarRendiciones()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new { ok = false, mensaje = "La sesion actual no posee un token valido." });
                }

                var request = new RendicionRequestDto
                {
                    adm_id = CajaActual?.AdmId ?? AdministracionId,
                    tipo = 'F'
                };

                _logger?.LogInformation(
                    "Cierre caja: cargando instrumentos finales. Adm={Adm}; Tipo={Tipo}; Usuario={Usuario}",
                    request.adm_id,
                    request.tipo,
                    UserName);

                var respuesta = await _rendicionServicio.CargarRendiciones(request, token);
                return Json(new
                {
                    ok = respuesta.Ok,
                    mensaje = respuesta.Mensaje,
                    lista = respuesta.ListaEntidad ?? []
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cierre caja: error cargando instrumentos finales. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudieron cargar los instrumentos de cierre." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CargarNominaciones([FromBody] RendicionNominalRequestDto request)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                if (request == null || string.IsNullOrWhiteSpace(request.ins_id))
                {
                    return Json(new { ok = false, mensaje = "Debe seleccionar un instrumento para cargar nominaciones." });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new { ok = false, mensaje = "La sesion actual no posee un token valido." });
                }

                request.adm_id = CajaActual?.AdmId ?? AdministracionId;

                _logger?.LogInformation(
                    "Cierre caja: cargando nominaciones. Adm={Adm}; Instrumento={Instrumento}; Usuario={Usuario}",
                    request.adm_id,
                    request.ins_id,
                    UserName);

                var respuesta = await _rendicionServicio.CargarNominaciones(request, token);
                return Json(new
                {
                    ok = respuesta.Ok,
                    mensaje = respuesta.Mensaje,
                    lista = respuesta.ListaEntidad ?? []
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cierre caja: error cargando nominaciones. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudieron cargar las nominaciones." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Confirmar([FromBody] CierreCajaConfirmarRequest request)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                if (request?.Rendiciones == null || request.Rendiciones.Count == 0)
                {
                    return Json(new { ok = false, mensaje = "No hay instrumentos de cierre para confirmar." });
                }

                var pendientes = await ObtenerPendientesDelCierreActual();
                if (!pendientes.Ok)
                {
                    return Json(new { ok = false, mensaje = pendientes.Mensaje });
                }

                if (pendientes.Pendientes.Count > 0)
                {
                    return Json(new
                    {
                        ok = false,
                        bloqueado = true,
                        mensaje = "No se puede cerrar la caja porque existen cobranzas diferidas pendientes.",
                        pendientes = pendientes.Pendientes
                    });
                }

                var caja = CajaActual;
                if (caja?.Caja == null || string.IsNullOrWhiteSpace(caja.CajaId))
                {
                    return Json(new { ok = false, mensaje = "Los datos de caja no estan disponibles." });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new { ok = false, mensaje = "La sesion actual no posee un token valido." });
                }

                var jsonRendiciones = JsonConvert.SerializeObject(request.Rendiciones);
                var total = request.Rendiciones.Sum(x => x.ins_importe);

                _logger?.LogInformation(
                    "Cierre caja: confirmando cierre. Caja={Caja}; Adm={Adm}; Usuario={Usuario}; Total={Total}; Registros={Registros}; JsonRendiciones={JsonRendiciones}",
                    caja.CajaId,
                    caja.AdmId ?? AdministracionId,
                    UserName,
                    total,
                    request.Rendiciones.Count,
                    jsonRendiciones);

                var respuesta = await _cajaServicio.CierreCajaConRendicion(new CierreCajaRequestDto
                {
                    caja_id = caja.CajaId ?? string.Empty,
                    usu_id = UserName ?? string.Empty,
                    adm_id = caja.AdmId ?? AdministracionId,
                    json_rendiciones = jsonRendiciones
                }, token);

                var entidad = respuesta.Entidad;

                _logger?.LogInformation(
                    "Cierre caja: response cierre. OkServicio={OkServicio}; Resultado={Resultado}; ResultadoId={ResultadoId}; Mensaje={Mensaje}",
                    respuesta.Ok,
                    entidad?.resultado,
                    entidad?.resultado_id,
                    entidad?.resultado_msj ?? respuesta.Mensaje);

                if (entidad == null)
                {
                    return Json(new { ok = false, mensaje = respuesta.Mensaje ?? "No se recibio respuesta al cerrar la caja." });
                }

                return Json(new
                {
                    ok = respuesta.Ok && entidad.resultado == 0,
                    resultado = entidad.resultado,
                    resultado_id = entidad.resultado_id,
                    mensaje = entidad.resultado_msj ?? respuesta.Mensaje,
                    usuario = UserName,
                    caja_id = caja.CajaId,
                    total_rendido = total,
                    datos = entidad
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cierre caja: error confirmando cierre. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "Error interno al confirmar el cierre de caja." });
            }
        }

        private async Task<(bool Ok, string Mensaje, List<FactPendienteResponseDto> Pendientes)> ObtenerPendientesDelCierreActual()
        {
            var (cajaOk, mensajeCaja, caja) = await AsegurarCajaHidratadaParaCierre();
            if (!cajaOk)
            {
                return (false, mensajeCaja, []);
            }

            var token = TokenCookie;
            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, "La sesion actual no posee un token valido.", []);
            }

            var proceso = caja.Caja.caja_nro_proceso ?? string.Empty;
            var cierreTexto = caja.Caja.caja_nro_cierre ?? string.Empty;
            _ = short.TryParse(cierreTexto, out var cierre);

            var request = new FactPendienteRequestDto
            {
                caja_nro_proceso = proceso,
                caja_nro_cierre = cierreTexto,
                cta_id = "%",
                tdo_codigo = string.Empty,
                cta_documento = "%",
                tipo_carga = "T"
            };

            _logger?.LogInformation(
                "Cierre caja: verificando facturas pendientes. Proceso={Proceso}; Cierre={Cierre}; Usuario={Usuario}; Request={Request}",
                proceso,
                cierreTexto,
                UserName,
                JsonConvert.SerializeObject(request));

            var respuesta = await _factDiferidaServicio.ObtenerFacturasPendientes(request, token);
            if (!respuesta.Ok && !respuesta.EsWarn)
            {
                return (false, respuesta.Mensaje ?? "No se pudieron obtener las cobranzas diferidas pendientes.", []);
            }

            if (respuesta.EsWarn)
            {
                _logger?.LogInformation(
                    "Cierre caja: la consulta de pendientes no devolvio registros. Se interpreta como cierre habilitado. Proceso={Proceso}; Cierre={Cierre}; Usuario={Usuario}; MensajeServicio={MensajeServicio}",
                    proceso,
                    cierre,
                    UserName,
                    respuesta.Mensaje);
            }

            var lista = respuesta.ListaEntidad ?? [];
            var pendientes = lista
                .Where(x => x.caja_nro_cierre == cierre
                    && (string.IsNullOrWhiteSpace(x.caja_nro_proceso)
                        || string.Equals(x.caja_nro_proceso.Trim(), proceso.Trim(), StringComparison.OrdinalIgnoreCase)))
                .ToList();

            _logger?.LogInformation(
                "Cierre caja: pendientes recibidos={Recibidos}; pendientes cierre actual={Pendientes}; Proceso={Proceso}; Cierre={Cierre}; Usuario={Usuario}",
                lista.Count,
                pendientes.Count,
                proceso,
                cierre,
                UserName);

            return (true, "OK", pendientes);
        }

        private async Task<(bool Ok, string Mensaje, gc.infraestructura.EntidadesComunes.Options.CajaSettings Caja)> AsegurarCajaHidratadaParaCierre()
        {
            var caja = CajaActual;
            if (caja?.Caja == null || string.IsNullOrWhiteSpace(caja.CajaId))
            {
                return (false, "Los datos de caja no estan disponibles.", caja ?? new());
            }

            if (!FaltanDatosOperativosDeCaja(caja))
            {
                return (true, "OK", caja);
            }

            var token = TokenCookie;
            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, "La sesion actual no posee un token valido.", caja);
            }

            _logger?.LogInformation(
                "Cierre caja: rehidratando datos operativos de caja. CajaId={CajaId}; Usuario={Usuario}; ProcesoActual={Proceso}; CierreActual={Cierre}",
                caja.CajaId,
                UserName,
                caja.Caja.caja_nro_proceso,
                caja.Caja.caja_nro_cierre);

            var respuesta = await _cajaServicio.ObtenerDatosCF(caja.CajaId, token);
            if (!respuesta.Ok || respuesta.Entidad == null)
            {
                _logger?.LogWarning(
                    "Cierre caja: no se pudo rehidratar caja. CajaId={CajaId}; Usuario={Usuario}; Mensaje={Mensaje}",
                    caja.CajaId,
                    UserName,
                    respuesta.Mensaje);

                return (false, respuesta.Mensaje ?? "No se pudieron obtener los datos operativos de caja.", caja);
            }

            caja.Caja = respuesta.Entidad;
            CajaActual = caja;

            if (FaltanDatosOperativosDeCaja(caja))
            {
                _logger?.LogWarning(
                    "Cierre caja: datos operativos incompletos luego de rehidratar. CajaId={CajaId}; Usuario={Usuario}; Proceso={Proceso}; Cierre={Cierre}; Operacion={Operacion}; Activa={Activa}",
                    caja.CajaId,
                    UserName,
                    caja.Caja.caja_nro_proceso,
                    caja.Caja.caja_nro_cierre,
                    caja.Caja.caja_nro_operacion,
                    caja.Caja.caja_activa);
            }

            return (true, "OK", caja);
        }

        private static bool FaltanDatosOperativosDeCaja(gc.infraestructura.EntidadesComunes.Options.CajaSettings caja)
        {
            return string.IsNullOrWhiteSpace(caja.Caja.caja_nro_proceso)
                || string.IsNullOrWhiteSpace(caja.Caja.caja_nro_cierre)
                || string.IsNullOrWhiteSpace(caja.Caja.caja_nro_operacion)
                || string.IsNullOrWhiteSpace(caja.Caja.caja_activa);
        }
        public sealed class CierreCajaConfirmarRequest
        {
            public List<CierreCajaRendicionItem> Rendiciones { get; set; } = [];
        }

        public sealed class CierreCajaRendicionItem
        {
            public string ins_id { get; set; } = string.Empty;
            public string ins_desc { get; set; } = string.Empty;
            public decimal ins_importe { get; set; }
        }
    }
}




