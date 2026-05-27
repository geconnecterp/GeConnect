using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class CheckoutController : ControladorBaseCaja
    {
        private readonly ICheckoutServicio _pagoFactServicio;

        public CheckoutController(IOptions<AppSettings> options,
            ICheckoutServicio pagoFactServicio,
            IHttpContextAccessor httpContext,
            ILogger<CheckoutController> logger) : base(options, httpContext, logger)
        {
            _pagoFactServicio = pagoFactServicio;

            InicializaBancos().GetAwaiter().GetResult();
        }

        private async Task InicializaBancos()
        {
            if (BancosLista.Count == 0 )
            {
               await ObtenerProveedores(_pagoFactServicio);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerValoresPendientes([FromBody] ValoresPendientesReqDto req)
        {
            try
            {
                if (req == null)
                {
                    _logger?.LogWarning("❌ Parámetro 'req' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar los valores para obtener los Valores Pendientes " });
                }

                if (string.IsNullOrEmpty(req.co_tipo))
                {
                    _logger?.LogWarning("❌ Parámetro 'co_tipo' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar el tipo de operación" });
                }

                if (string.IsNullOrEmpty(req.cta_id))
                {
                    _logger?.LogWarning("❌ Parámetro 'cta_id' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar el id de la cuenta" });
                }
                if (string.IsNullOrEmpty(req.adm_id))
                {
                    _logger?.LogWarning("❌ Parámetro 'adm_id' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar el id del administrador" });
                }

                var res = await _pagoFactServicio.ObtenerValoresPendientes(req, TokenCookie);
                if (res == null)
                {
                    _logger?.LogWarning("❌ No se encontraron valores pendientes para los parámetros proporcionados");
                    return Json(new { ok = false, mensaje = "No se encontraron valores pendientes para los parámetros proporcionados" });
                }
                if (!res.Ok)
                {
                    if (res.EsError)
                    {
                        _logger?.LogError("❌ Error al obtener valores pendientes: {Mensaje}", res.Mensaje);
                        return Json(new { ok = false, error = true, warn = false, mensaje = res.Mensaje ?? "Ocurrió un error al obtener los valores pendientes" });
                    }
                    else
                    {
                        _logger?.LogWarning("⚠️ Advertencia al obtener valores pendientes: {Mensaje}", res.Mensaje);
                        return Json(new { ok = false, error = false, warn = true, mensaje = res.Mensaje ?? "Ocurrió una advertencia al obtener los valores pendientes" });
                    }

                }
                return Json(new { ok = true, error = false, warn = false, mensaje = "Valores pendientes obtenidos correctamente", datos = res.ListaEntidad });

            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Excepción al obtener valores pendientes");
                return Json(new { ok = false, error = true, warn = false, mensaje = "Ocurrió un error al obtener los valores pendientes" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerValoresNC([FromBody] ValoresNCReqDto req)
        {
            try
            {
                if (req == null)
                {
                    _logger?.LogWarning("❌ Parámetro 'req' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar los valores para obtener los Valores NC " });
                }
                if (string.IsNullOrEmpty(req.co_tipo))
                {
                    _logger?.LogWarning("❌ Parámetro 'co_tipo' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar el tipo de operación" });
                }
                if (string.IsNullOrEmpty(req.cta_id))
                {
                    _logger?.LogWarning("❌ Parámetro 'cta_id' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar el id de la cuenta" });
                }
                if (string.IsNullOrEmpty(req.adm_id))
                {
                    _logger?.LogWarning("❌ Parámetro 'adm_id' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar el id del administrador" });
                }
                var res = await _pagoFactServicio.ObtenerValoresNC(req, TokenCookie);
                if (res == null)
                {
                    _logger?.LogWarning("❌ No se encontraron valores NC para los parámetros proporcionados");
                    return Json(new { ok = false, mensaje = "No se encontraron valores NC para los parámetros proporcionados" });
                }

                if (!res.Ok)
                {
                    if (res.EsError)
                    {
                        _logger?.LogError("❌ Error al obtener valores NC: {Mensaje}", res.Mensaje);
                        return Json(new { ok = false, error = true, warn = false, mensaje = res.Mensaje ?? "Ocurrió un error al obtener los valores NC" });
                    }
                    else
                    {
                        _logger?.LogWarning("⚠️ Advertencia al obtener valores NC: {Mensaje}", res.Mensaje);
                        return Json(new { ok = false, error = false, warn = true, mensaje = res.Mensaje ?? "Ocurrió una advertencia al obtener los valores NC" });
                    }
                }
                return Json(new { ok = true, error = false, warn = false, mensaje = "Valores NC obtenidos correctamente", datos = res.ListaEntidad });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Excepción al obtener valores NC");
                return Json(new { ok = false, error = true, warn = false, mensaje = "Ocurrió un error al obtener los valores NC" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerValoresMP([FromBody] ValoresMPReqDto req)
        {
            try
            {
                // ❶ Validar parámetros básicos
                if (req == null)
                {
                    _logger?.LogWarning("❌ Parámetro 'req' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar los valores para obtener los Valores MP" });
                }

                if (string.IsNullOrEmpty(req.co_tipo))
                {
                    _logger?.LogWarning("❌ Parámetro 'co_tipo' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar el tipo de operación" });
                }
                var cli = ClienteActual;
                
                if (cli ==null || (string.IsNullOrEmpty(cli.cta_id) && string.IsNullOrEmpty(cli.cta_documento)))
                {
                    _logger?.LogWarning("❌ El identificador del cliente es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar el id de la cuenta" });
                }
                req.cta_id = cli.Origen=="C"? cli.cta_id:cli.cta_documento;
                // ❷ ✅ NUEVO: Obtener adm_id desde la sesión (NO desde el request)
                // El controlador hereda de ControladorBase que ya tiene AdministracionId
                if (string.IsNullOrEmpty(AdministracionId))
                {
                    _logger?.LogError("❌ CRÍTICO: AdministracionId no disponible en sesión");
                    return Json(new { ok = false, mensaje = "Datos de sesión incompletos. Por favor, recargue la página." });
                }

                // ❸ Asignar adm_id desde la sesión del servidor
                req.adm_id = AdministracionId;

                _logger?.LogInformation(
                    "[ObtenerValoresMP] Usuario: {UserName}, Adm: {AdmId}, co_tipo: {CoTipo}, cta_id: {CtaId}",
                    UserName,
                    req.adm_id,
                    req.co_tipo,
                    req.cta_id
                );

                // ❹ Llamar al servicio
                var res = await _pagoFactServicio.ObtenerValoresMP(req, TokenCookie);

                if (res == null)
                {
                    _logger?.LogWarning("❌ No se encontraron valores MP para los parámetros proporcionados");
                    return Json(new { ok = false, mensaje = "No se encontraron valores MP para los parámetros proporcionados" });
                }

                if (!res.Ok)
                {
                    if (res.EsError)
                    {
                        _logger?.LogError("❌ Error al obtener valores MP: {Mensaje}", res.Mensaje);
                        return Json(new { ok = false, error = true, warn = false, mensaje = res.Mensaje ?? "Ocurrió un error al obtener los valores MP" });
                    }
                    else
                    {
                        _logger?.LogWarning("⚠️ Advertencia al obtener valores MP: {Mensaje}", res.Mensaje);
                        return Json(new { ok = false, error = false, warn = true, mensaje = res.Mensaje ?? "Ocurrió una advertencia al obtener los valores MP" });
                    }
                }

                return Json(new { ok = true, error = false, warn = false, mensaje = "Valores MP obtenidos correctamente", datos = res.ListaEntidad });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Excepción al obtener valores MP");
                return Json(new { ok = false, error = true, warn = false, mensaje = "Ocurrió un error al obtener los valores MP" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerValoresIns([FromBody] ValoresInsReqDto req)
        {
            try
            {
                if (req == null)
                {
                    _logger?.LogWarning("❌ Parámetro 'req' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar los valores para obtener los Valores Ins " });
                }
                if (string.IsNullOrEmpty(req.co_tipo))
                {
                    _logger?.LogWarning("❌ Parámetro 'co_tipo' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar el tipo de operación" });
                }
                var cli = ClienteActual;

                if (cli == null || (string.IsNullOrEmpty(cli.cta_id) && string.IsNullOrEmpty(cli.cta_documento)))
                {
                    _logger?.LogWarning("❌ El identificador del cliente es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar el id de la cuenta" });
                }
                req.cta_id = cli.Origen == "C" ? cli.cta_id : cli.cta_documento;
                // ❷ ✅ NUEVO: Obtener adm_id desde la sesión (NO desde el request)
                // El controlador hereda de ControladorBase que ya tiene AdministracionId
                if (string.IsNullOrEmpty(AdministracionId))
                {
                    _logger?.LogError("❌ CRÍTICO: AdministracionId no disponible en sesión");
                    return Json(new { ok = false, mensaje = "Datos de sesión incompletos. Por favor, recargue la página." });
                }

                // ❸ Asignar adm_id desde la sesión del servidor
                req.adm_id = AdministracionId;
                var res = await _pagoFactServicio.ObtenerValoresIns(req, TokenCookie);
                if (res == null)
                {
                    _logger?.LogWarning("❌ No se encontraron valores Ins para los parámetros proporcionados");
                    return Json(new { ok = false, mensaje = "No se encontraron valores Ins para los parámetros proporcionados" });
                }
                if (!res.Ok)
                {
                    if (res.EsError)
                    {
                        _logger?.LogError("❌ Error al obtener valores Ins: {Mensaje}", res.Mensaje);
                        return Json(new { ok = false, error = true, warn = false, mensaje = res.Mensaje ?? "Ocurrió un error al obtener los valores Ins" });
                    }
                    else
                    {
                        _logger?.LogWarning("⚠️ Advertencia al obtener valores Ins: {Mensaje}", res.Mensaje);
                        return Json(new { ok = false, error = false, warn = true, mensaje = res.Mensaje ?? "Ocurrió una advertencia al obtener los valores Ins" });
                    }
                }
                return Json(new { ok = true, error = false, warn = false, mensaje = "Valores Ins obtenidos correctamente", datos = res.ListaEntidad });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Excepción al obtener valores Ins");
                return Json(new { ok = false, error = true, warn = false, mensaje = "Ocurrió un error al obtener los valores Ins" });

            }
        }
    }
}
