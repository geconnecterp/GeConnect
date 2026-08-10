using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class CambioValoresController : ControladorBaseCaja
    {
        private const string MODULO = "CambioValores";
        private const string MODULO_DESC = "Cambios e Ingresos de Valores";
        private const string CO_TIPO_CONSULTA = "CV";

        private readonly ICajaInitServicio _cajaInitServicio;
        private readonly ICheckoutServicio _checkoutServicio;

        public CambioValoresController(
            IOptions<AppSettings> options,
            ILogger<CambioValoresController> logger,
            IHttpContextAccessor httpContext,
            ICajaInitServicio cajaInitServicio,
            ICheckoutServicio checkoutServicio)
            : base(options, httpContext, logger)
        {
            _cajaInitServicio = cajaInitServicio;
            _checkoutServicio = checkoutServicio;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                return redirectResult;
            }

            var caja = CajaActual;
            if (caja?.Caja == null || string.IsNullOrWhiteSpace(caja.CajaId))
            {
                TempData["Error"] = "No se encontraron datos validos de caja para iniciar cambios e ingresos de valores.";
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }

            ViewBag.Modulo = MODULO;
            ViewBag.ModuloDesc = MODULO_DESC;
            ViewBag.Usuario = UserName;
            ViewBag.CajaId = caja.CajaId;
            ViewBag.CajaNombre = caja.Caja.caja_nombre ?? string.Empty;
            ViewBag.Proceso = caja.Caja.caja_nro_proceso ?? string.Empty;
            ViewBag.Cierre = caja.Caja.caja_nro_cierre ?? string.Empty;

            return View();
        }

        [HttpPost]
        public JsonResult ValidacionInicial()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { success = false, message = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                var caja = CajaActual;
                if (caja?.Caja == null || string.IsNullOrWhiteSpace(caja.CajaId))
                {
                    return Json(new { success = false, message = "Los datos de caja no estan disponibles." });
                }

                var (esValido, mensajeValidacion) = _cajaInitServicio.ValidarDatosIniciales(caja);
                if (!esValido)
                {
                    _logger?.LogWarning(
                        "Cambio valores: validacion inicial fallida. CajaId={CajaId}; Usuario={Usuario}; Motivo={Motivo}",
                        caja.CajaId,
                        UserName,
                        mensajeValidacion);

                    return Json(new { success = false, message = mensajeValidacion });
                }

                _logger?.LogInformation(
                    "Cambio valores: validacion inicial exitosa. CajaId={CajaId}; Usuario={Usuario}",
                    caja.CajaId,
                    UserName);

                return Json(new { success = true, message = "Caja validada correctamente." });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cambio valores: error inesperado en ValidacionInicial. Usuario={Usuario}", UserName);
                return Json(new { success = false, message = "Error interno al validar los datos de caja." });
            }
        }

        [HttpPost]
        public JsonResult ValidarClienteRegistrado([FromBody] string cta_id)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, hayDatos = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                var cliente = ClienteActual;
                var ctaId = (cta_id ?? string.Empty).Trim();
                var clienteCtaId = (cliente?.cta_id ?? string.Empty).Trim();
                var origen = (cliente?.Origen ?? string.Empty).Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(ctaId) && !string.IsNullOrWhiteSpace(clienteCtaId))
                {
                    ctaId = clienteCtaId;
                }

                if (string.IsNullOrWhiteSpace(ctaId) || origen == "F")
                {
                    return Json(new { ok = false, hayDatos = false, mensaje = "Este modulo requiere un Cliente Registrado con identificador de cuenta valido." });
                }

                return Json(new { ok = true, hayDatos = true, mensaje = "Cliente registrado validado." });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cambio valores: error validando cliente registrado. Usuario={Usuario}", UserName);
                return Json(new { ok = false, hayDatos = false, mensaje = "No se pudo validar el cliente seleccionado." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ObtenerMediosPago()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                if (!TryObtenerClienteRegistrado(out var ctaId, out var mensajeCliente))
                {
                    return Json(new { ok = false, mensaje = mensajeCliente });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new { ok = false, mensaje = "La sesion actual no posee un token valido." });
                }

                var request = new ValoresMPReqDto
                {
                    co_tipo = CO_TIPO_CONSULTA,
                    cta_id = ctaId,
                    adm_id = AdministracionId ?? string.Empty
                };

                _logger?.LogInformation(
                    "Cambio valores: consultando medios habilitados. Cta={Cta}; CoTipo={CoTipo}; Adm={Adm}; Usuario={Usuario}",
                    request.cta_id,
                    request.co_tipo,
                    request.adm_id,
                    UserName);

                var respuesta = await _checkoutServicio.ObtenerValoresMP(request, token);
                var lista = respuesta?.ListaEntidad ?? [];

                _logger?.LogInformation(
                    "Cambio valores: respuesta medios habilitados. Ok={Ok}; Registros={Registros}; Mensaje={Mensaje}",
                    respuesta?.Ok,
                    lista.Count,
                    respuesta?.Mensaje);

                return Json(new
                {
                    ok = respuesta?.Ok == true,
                    mensaje = respuesta?.Ok == true ? "OK" : respuesta?.Mensaje,
                    datos = lista
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cambio valores: error obteniendo medios habilitados. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudieron obtener los medios de pago habilitados." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ObtenerInstrumentos([FromBody] ValoresInsReqDto request)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                if (request == null || string.IsNullOrWhiteSpace(request.tcf_id))
                {
                    return Json(new { ok = false, mensaje = "Debe seleccionar un medio de pago." });
                }

                if (!TryObtenerClienteRegistrado(out var ctaId, out var mensajeCliente))
                {
                    return Json(new { ok = false, mensaje = mensajeCliente });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new { ok = false, mensaje = "La sesion actual no posee un token valido." });
                }

                request.co_tipo = CO_TIPO_CONSULTA;
                request.cta_id = ctaId;
                request.adm_id = AdministracionId ?? string.Empty;

                _logger?.LogInformation(
                    "Cambio valores: consultando instrumentos. Cta={Cta}; Tcf={Tcf}; CoTipo={CoTipo}; Adm={Adm}; Usuario={Usuario}",
                    request.cta_id,
                    request.tcf_id,
                    request.co_tipo,
                    request.adm_id,
                    UserName);

                var respuesta = await _checkoutServicio.ObtenerValoresIns(request, token);
                var lista = respuesta?.ListaEntidad ?? [];

                _logger?.LogInformation(
                    "Cambio valores: respuesta instrumentos. Ok={Ok}; Registros={Registros}; Mensaje={Mensaje}",
                    respuesta?.Ok,
                    lista.Count,
                    respuesta?.Mensaje);

                return Json(new
                {
                    ok = respuesta?.Ok == true,
                    mensaje = respuesta?.Ok == true ? "OK" : respuesta?.Mensaje,
                    datos = lista
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cambio valores: error obteniendo instrumentos. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudieron obtener los instrumentos habilitados." });
            }
        }

        [HttpPost]
        public JsonResult PrepararConfirmacion([FromBody] CambioValoresConfirmarRequest request)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                if (!TryObtenerClienteRegistrado(out var ctaId, out var mensajeCliente))
                {
                    return Json(new { ok = false, mensaje = mensajeCliente });
                }

                var valores = request?.Valores ?? [];
                if (valores.Count == 0)
                {
                    return Json(new { ok = false, mensaje = "Debe cargar al menos un valor antes de finalizar." });
                }

                var tipo = string.Equals(request?.Tipo, "IV", StringComparison.OrdinalIgnoreCase) ? "IV" : "CV";
                var total = valores.Sum(x => x.rb_importe);
                var jsonValores = JsonConvert.SerializeObject(valores);

                _logger?.LogInformation(
                    "Cambio valores: confirmacion pendiente preparada. Cta={Cta}; Tipo={Tipo}; Registros={Registros}; Total={Total}; JsonValores={JsonValores}; Usuario={Usuario}",
                    ctaId,
                    tipo,
                    valores.Count,
                    total,
                    jsonValores,
                    UserName);

                return Json(new
                {
                    ok = false,
                    pendiente = true,
                    mensaje = "La carga de valores quedo preparada, pero la confirmacion esta pendiente hasta que este disponible SPGECO_CAJA_Ope_Cv_IV.",
                    tipo,
                    total,
                    json_valores = jsonValores
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cambio valores: error preparando confirmacion. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudo preparar la confirmacion de valores." });
            }
        }

        private bool TryObtenerClienteRegistrado(out string ctaId, out string mensaje)
        {
            var cliente = ClienteActual;
            ctaId = (cliente?.cta_id ?? string.Empty).Trim();
            var origen = (cliente?.Origen ?? string.Empty).Trim().ToUpperInvariant();

            if (cliente == null || string.IsNullOrWhiteSpace(ctaId) || origen == "F")
            {
                mensaje = "Debe seleccionar un Cliente Registrado con identificador de cuenta valido.";
                return false;
            }

            mensaje = string.Empty;
            return true;
        }

        public sealed class CambioValoresConfirmarRequest
        {
            public string Tipo { get; set; } = "CV";
            public List<Json_Valor> Valores { get; set; } = [];
        }
    }
}
