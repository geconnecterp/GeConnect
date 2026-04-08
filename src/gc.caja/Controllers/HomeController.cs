using gc.caja.core.Servicios.Contratos.Cajas;
using gc.caja.Models;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace gc.caja.Controllers
{
    public class HomeController : ControladorBase
    {
        private new readonly IHttpContextAccessor _context;
        private readonly ICajaServicio _caja;

        public HomeController(
            ILogger<HomeController> logger,
            IOptions<AppSettings> options,
            IHttpContextAccessor context,
            ICajaServicio cajaService) : base(options, context, logger)
        {
            _context = context;
            _caja = cajaService;
        }

        public IActionResult Index()
        {
            if (UserPerfiles.Count() == 0)
            {
                return RedirectToAction("login", "token", new { area = "seguridad" });
            }

            return View();
        }

        /// <summary>
        /// Validación integrada de usuario y caja
        /// Resultado = 0: Requiere apertura
        /// Resultado = 3: Evaluar opciones (apertura, opera sin PV, salir)
        /// Resultado = 4: Requiere cambio de PV
        /// Otro: Error - Salir
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ValidacionIntegridad()
        {
            try
            {
                var cajaActual = CajaActual;

                if (string.IsNullOrEmpty(cajaActual?.CajaId))
                {
                    return Json(new
                    {
                        ok = false,
                        resultado = -1,
                        mensaje = "No se ha configurado una caja para esta estación.",
                        usuario = UserName,
                        caja_id = string.Empty,
                        respuesta_id = string.Empty
                    });
                }

                var result = await _caja.ValidarIntegridadUsuarioCaja(new CajaReqDto
                {
                    usu_id = UserName,
                    caja_id = cajaActual.CajaId,
                    adm_id = AdministracionId,
                }, TokenCookie);

                if (!result.Ok)
                {
                    return Json(new
                    {
                        ok = false,
                        resultado = result.Entidad?.resultado ?? -1,
                        mensaje = result.Mensaje ?? "Error desconocido al validar integridad.",
                        usuario = UserName,
                        caja_id = cajaActual.CajaId,
                        respuesta_id = result.Entidad?.resultado_id ?? string.Empty
                    });
                }

                return Json(new
                {
                    ok = true,
                    resultado = result.Entidad?.resultado ?? 0,
                    mensaje = result.Entidad?.resultado_msj ?? "Validación exitosa.",
                    usuario = UserName,
                    caja_id = cajaActual.CajaId,
                    respuesta_id = result.Entidad?.resultado_id ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al validar integridad de caja");

                return Json(new
                {
                    ok = false,
                    resultado = -999,
                    mensaje = "Error interno al validar integridad. Por favor, contacte al administrador.",
                    usuario = UserName,
                    caja_id = CajaActual?.CajaId ?? string.Empty,
                    respuesta_id = string.Empty
                });
            }
        }

        /// <summary>
        /// Realiza la apertura de caja
        /// Resultado = 0: Caja abierta correctamente - Obtener datos
        /// Resultado = 3: Caja ya abierta - Menú con solo botón CIERRE activo
        /// Otro: Error - Salir
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> AperturaCaja()
        {
            try
            {
                var cajaActual = CajaActual;

                if (string.IsNullOrEmpty(cajaActual?.CajaId))
                {
                    return Json(new
                    {
                        ok = false,
                        resultado = -1,
                        mensaje = "No se ha configurado una caja para esta estación.",
                        usuario = UserName,
                        caja_id = string.Empty
                    });
                }

                var result = await _caja.AperturaCaja(new CajaReqDto
                {
                    usu_id = UserName,
                    caja_id = cajaActual.CajaId,
                    adm_id = AdministracionId,
                }, TokenCookie);

                if (!result.Ok)
                {
                    return Json(new
                    {
                        ok = false,
                        resultado = result.Entidad?.resultado ?? -1,
                        mensaje = result.Mensaje ?? "Error al realizar apertura de caja.",
                        usuario = UserName,
                        caja_id = cajaActual.CajaId
                    });
                }

                return Json(new
                {
                    ok = true,
                    resultado = result.Entidad?.resultado ?? 0,
                    mensaje = result.Entidad?.resultado_msj ?? "Apertura de caja exitosa.",
                    usuario = UserName,
                    caja_id = cajaActual.CajaId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar apertura de caja");

                return Json(new
                {
                    ok = false,
                    resultado = -999,
                    mensaje = "Error interno al realizar apertura de caja. Por favor, contacte al administrador.",
                    usuario = UserName,
                    caja_id = CajaActual?.CajaId ?? string.Empty
                });
            }
        }

        /// <summary>
        /// Obtiene los datos de la caja/PV después de apertura exitosa
        /// Resultado = 0: Datos obtenidos - Menú con acceso completo
        /// Otro: Error - Salir
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ObtenerDatosCaja()
        {
            try
            {
                var cajaActual = CajaActual;

                if (string.IsNullOrEmpty(cajaActual?.CajaId))
                {
                    return Json(new
                    {
                        ok = false,
                        resultado = -1,
                        mensaje = "No se ha configurado una caja para esta estación.",
                        //datos = (object)null
                    });
                }

                var result = await _caja.ObtenerDatosCF(cajaActual.CajaId, TokenCookie);

                if (!result.Ok || result.Entidad == null)
                {
                    return Json(new
                    {
                        ok = false,
                        resultado = -1,
                        mensaje = result.Mensaje ?? "Error al obtener datos de caja.",
                       //datos = (object)null
                    });
                }

                return Json(new
                {
                    ok = true,
                    resultado = 0,
                    mensaje = "Datos obtenidos exitosamente.",
                    datos = new
                    {
                        caja_id = result.Entidad.caja_id,
                        caja_nombre = result.Entidad.caja_nombre,
                        depo_id = result.Entidad.depo_id,
                        usuario = UserName,
                        administracion = AdministracionId,
                        dia_movi = result.Entidad.dia_movi,
                        caja_estado = result.Entidad.caja_estado,
                        caja_nro_proceso = result.Entidad.caja_nro_proceso
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos de caja");

                return Json(new
                {
                    ok = false,
                    resultado = -999,
                    mensaje = "Error interno al obtener datos de caja.",
                    datos = (object)null
                });
            }
        }

        /// <summary>
        /// Realiza el cambio de punto de venta
        /// Resultado = 0: Cambio exitoso - Continuar con Apertura
        /// Otro: Error - Salir
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CambioPuntoVenta(string nuevo_pv_id)
        {
            try
            {
                var cajaActual = CajaActual;

                if (string.IsNullOrEmpty(cajaActual?.CajaId) || string.IsNullOrEmpty(nuevo_pv_id))
                {
                    return Json(new
                    {
                        ok = false,
                        resultado = -1,
                        mensaje = "Parámetros inválidos para cambio de punto de venta.",
                        usuario = UserName
                    });
                }

                // TODO: Implementar lógica de cambio de PV cuando exista el SP correspondiente
                // Por ahora retornamos un placeholder

                return Json(new
                {
                    ok = true,
                    resultado = 0,
                    mensaje = "Cambio de punto de venta exitoso.",
                    usuario = UserName,
                    nuevo_pv_id = nuevo_pv_id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar punto de venta");

                return Json(new
                {
                    ok = false,
                    resultado = -999,
                    mensaje = "Error interno al cambiar punto de venta.",
                    usuario = UserName
                });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
