using gc.caja.core.Servicios.Contratos.Cajas;
using gc.caja.Models;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;

namespace gc.caja.Controllers
{
    public class HomeController : ControladorBaseCaja
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
            if (!VerificarAutenticacion(out IActionResult redirectResult))
                return redirectResult;

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
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, resultado = -1, mensaje = "Sesión expirada" });

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

                #region Mock para que si viene en 0 o 3 ponerlo en 4 para emular el cambio de Puesto de venta
                //if (result.Ok && result.Entidad != null && (result.Entidad.resultado == 3 || result.Entidad.resultado == 0))
                //{
                //    // Solo para pruebas, forzamos a que el resultado sea 0 para simular apertura exitosa
                //    result.Entidad.resultado = 4;
                //    result.Entidad.resultado_msj = "Cambio de PV";
                //}
                #endregion

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
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, resultado = -1, mensaje = "Sesión expirada" });

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

                #region Mock - forzamos si es 3 a que este en 0

                //if(result.Ok && result.Entidad != null && result.Entidad.resultado == 3)
                //{
                //    // Solo para pruebas, forzamos a que el resultado sea 0 para simular apertura exitosa
                //    result.Entidad.resultado = 0;
                //    result.Entidad.resultado_msj = "Apertura de caja exitosa (mock).";
                //}
                #endregion


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
                _logger?.LogError(ex, "Error al realizar apertura de caja");

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
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, resultado = -1, mensaje = "Sesión expirada" });

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

                //resguardo los datos desde el sp
                var caja = CajaActual;
                caja.Caja = result.Entidad;
                CajaActual = caja;

                // ═══════════════════════════════════════════════════════════
                // ✅ NUEVO v2.0: VALIDACIÓN DE ESTADO DEL PUNTO DE VENTA
                // ═══════════════════════════════════════════════════════════

                var validacionPV = await ValidarEstadoPuntoVenta(
                    cajaServicio: _caja,
                    cajaId: cajaActual.CajaId,
                    ctrlId: caja.Caja.ctrl_id,
                    nroProceso: result?.Entidad?.caja_nro_proceso,
                    nroCierre: result?.Entidad?.caja_nro_cierre,
                    tipoLlamada: "I" // ← "I" = Inicio
                );

                // Si no puede continuar, retornar error
                if (!validacionPV.PuedeContinuar)
                {
                    return Json(new
                    {
                        ok = false,
                        resultado = validacionPV.Resultado,
                        mensaje = validacionPV.Mensaje,
                        mostrar_mensaje = true
                    });
                }

                // Preparar respuesta exitosa
                var respuestaExito = new
                {
                    ok = true,
                    resultado = 0,
                    mensaje = "Datos obtenidos exitosamente.",
                    datos = new
                    {
                        caja_id = result?.Entidad?.caja_id,
                        caja_nombre = result?.Entidad?.caja_nombre,
                        depo_id = result?.Entidad?.depo_id,
                        usuario = UserName,
                        administracion = AdministracionId,
                        dia_movi = result?.Entidad?.dia_movi,
                        caja_estado = result?.Entidad?.caja_estado,
                        caja_nro_proceso = result?.Entidad?.caja_nro_proceso
                    }
                };

                // Si hay advertencia, agregarla
                if (validacionPV.EsAdvertencia)
                {
                    return Json(new
                    {
                        respuestaExito.ok,
                        respuestaExito.resultado,
                        respuestaExito.mensaje,
                        respuestaExito.datos,
                        mensaje_advertencia = validacionPV.Mensaje,
                        mostrar_mensaje = true
                    });
                }

                return Json(respuestaExito);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener datos de caja");

                return Json(new
                {
                    ok = false,
                    resultado = -999,
                    mensaje = "Error interno al obtener datos de caja.",
                    datos = ""
                });
            }
        }

        /// <summary>
        /// Realiza el cambio de punto de venta
        /// Resultado = 0: Cambio exitoso - Continuar con Apertura
        /// Resultado = -1: Funcionalidad no implementada (MOCK)
        /// Otro: Error - Salir
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CambioPuntoVenta(string nuevo_pv_id)
        {
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, resultado = -1, mensaje = "Sesión expirada" });

                var cajaActual = CajaActual;

                // Validación de parámetros
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

                // MOCK: Funcionalidad no implementada aún
                _logger?.LogWarning("CambioPuntoVenta llamado pero funcionalidad no implementada (MOCK). Usuario: {Usuario}, CajaId: {CajaId}, NuevoPvId: {NuevoPvId}", 
                    UserName, cajaActual.CajaId, nuevo_pv_id ?? "null");

                return Json(new
                {
                    ok = false,
                    resultado = -1,
                    mensaje = "MOCK - El Cambio de PV aún no puede ser ejecutado (TODO).",
                    usuario = UserName,
                    caja_id = cajaActual.CajaId
                });

                // TODO: Implementar lógica real cuando exista el SP correspondiente
                /*
                var result = await _caja.CambiarPuntoVenta(new CajaReqDto
                {
                    usu_id = UserName,
                    caja_id = cajaActual.CajaId,
                    adm_id = AdministracionId,
                    nuevo_pv_id = nuevo_pv_id
                }, TokenCookie);

                if (!result.Ok)
                {
                    return Json(new
                    {
                        ok = false,
                        resultado = result.Entidad?.resultado ?? -1,
                        mensaje = result.Mensaje ?? "Error al cambiar punto de venta.",
                        usuario = UserName,
                        caja_id = cajaActual.CajaId
                    });
                }

                return Json(new
                {
                    ok = true,
                    resultado = result.Entidad?.resultado ?? 0,
                    mensaje = result.Entidad?.resultado_msj ?? "Cambio de punto de venta exitoso.",
                    usuario = UserName,
                    caja_id = cajaActual.CajaId,
                    nuevo_pv_id = nuevo_pv_id
                });
                */
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cambiar punto de venta");

                return Json(new
                {
                    ok = false,
                    resultado = -999,
                    mensaje = "Error interno al cambiar punto de venta.",
                    usuario = UserName,
                    caja_id = CajaActual?.CajaId ?? string.Empty
                });
            }
        }

        /// <summary>
        /// Realiza el cierre de caja
        /// Resultado = 0: Cierre exitoso - Mostrar resumen y redirigir a login
        /// Otro: Error - Mostrar mensaje de error
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CierreCaja()
        {
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, resultado = -1, mensaje = "Sesión expirada" });

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

                var result = await _caja.CierreCaja(new CajaReqDto
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
                        mensaje = result.Mensaje ?? "Error al realizar cierre de caja.",
                        usuario = UserName,
                        caja_id = cajaActual.CajaId
                    });
                }

                // Extraer datos del cierre si existen
                var entidad = result.Entidad;
                
                return Json(new
                {
                    ok = true,
                    resultado = entidad?.resultado ?? 0,
                    mensaje = entidad?.resultado_msj ?? "Cierre de caja exitoso.",
                    usuario = UserName,
                    caja_id = cajaActual.CajaId,
                    // Datos adicionales del cierre (estructura flexible)
                    datos = entidad
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al realizar cierre de caja");

                return Json(new
                {
                    ok = false,
                    resultado = -999,
                    mensaje = "Error interno al realizar cierre de caja. Por favor, contacte al administrador.",
                    usuario = UserName,
                    caja_id = CajaActual?.CajaId ?? string.Empty
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
