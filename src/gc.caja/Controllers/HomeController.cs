using gc.caja.core.Servicios.Contratos.Cajas;
using gc.caja.Models;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using log4net.Util;
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
        /// Retorna: 
        /// - resultado < 0: Error crítico
        /// - resultado = 0: OK, todo correcto
        /// - resultado > 0: Advertencia/Información
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ValidacionIntegridad()
        {
            try
            {
                var cajaActual = CajaActual;

                // Validar que exista configuración de caja
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

                // Llamar al servicio de validación
                var result = await _caja.ValidarIntegridadUsuarioCaja(new CajaValidaReqDto
                {
                    usu_id = UserName,
                    caja_id = cajaActual.CajaId,
                    adm_id = AdministracionId,
                },TokenCookie);

                #region Borrar esta region luego que se recepcionen los valores correctos

                ///se presentan los casos
                ///caso 1 PV no tiene nro de cierre asociado (respuesta_id) y se debe hacer una apertura: 
                ///respuesta = 0 - respuesta_msj = "" - respuesta_id = ""
                ///------------------------------------------------------------
                ///Caso 2 PV tiene nro de cierre asociado. Tiene que cerrar la Caja.
                ///respuesta = 0 - respuesta_msj = "" - respuesta_id = "99-9999999"


                if (!result.Ok)
                {
                    result.Ok = true;
                    result.Entidad.resultado = 0;
                    result.Entidad.resultado_id = "";                    
                }
                #endregion

                if (!result.Ok)
                {
                    // Error en la validación
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
                else                
                {                  
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
                    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar integridad de caja");

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
        /// Realiza la apertura de caja para el usuario y caja actual
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> AperturaCaja()
        {
            try
            {
                var cajaActual = CajaActual;

                // Validar que exista configuración de caja
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

                // Llamar al servicio de apertura
                var result = await _caja.AperturaCaja(new CajaValidaReqDto
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

        [HttpPost]
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
