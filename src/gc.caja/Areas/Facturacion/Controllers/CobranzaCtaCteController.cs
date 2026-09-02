using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Formats.Asn1;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class CobranzaCtaCteController : ControladorBaseCaja
    {
        private readonly string Co_TipoCC;
        private readonly ICtaCteServicio _ctaCteServicio;
        private const string MODULO = "CobranzaCtaCte";
        private const string MODULO_DESC = "Módulo de Cobranza de Cuenta Corriente";

        public CobranzaCtaCteController(IOptions<AppSettings> options,
            IHttpContextAccessor contexto, ILogger<CobranzaCtaCteController> logger,
            ICtaCteServicio ctaCteServicio) : base(options, contexto, logger)
        {
            Co_TipoCC = "CC";
            _ctaCteServicio = ctaCteServicio;
        }

        public IActionResult Index()
        {
            _logger?.LogInformation("═══════════════════════════════════════════════════");
            _logger?.LogInformation($"🚀 INICIANDO {MODULO_DESC} v1.0");
            _logger?.LogInformation("═══════════════════════════════════════════════════");

            // ❶ VALIDAR AUTENTICACIÓN
            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                _logger?.LogWarning("❌ Usuario no autenticado, redirigiendo a login");
                return redirectResult;
            }

            ViewBag.Co_TipoCC = Co_TipoCC;
            ViewBag.ModuloCC = MODULO;
            ViewBag.ModuloDesc = MODULO_DESC;

            return View();
        }

        [HttpPost]
        public IActionResult Validar()
        {
            // ❶ VALIDAR AUTENTICACIÓN
            if (!VerificarAutenticacion(out _))
            {
                return Json(new { success = false, mensaje = "Sesión expirada." });
            }

            return Json(new { success = true, message = "Acceso permitido" });
        }

        /// <summary>
        /// Esta action tiene la misión de ir a buscar cuenta corriente
        /// </summary>
        /// <param name="cta_id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> VerificaExistenciaCuentaCorriente([FromBody]string cta_id)
        {
            try
            {
                if (string.IsNullOrEmpty(cta_id))
                {
                    _logger?.LogWarning("❌ clienteId vacío o nulo");
                    return Json(new { ok = false, mensaje = "El ID del cliente es requerido." });
                }

                var adm_id = AdministracionId;
                var resp = await _ctaCteServicio.ObtenerCtaCte(cta_id, adm_id, TokenCookie);

                if (resp.Ok)
                {
                    if(resp.ListaEntidad?.Any() == true)
                    {
                        _logger?.LogInformation($"✅ Se encontraron {resp.ListaEntidad.Count} registros de la cuenta corriente de la cuenta {cta_id}");
                        //resguardamos la información en la sesión para poder usarla posteriormente
                        CuentaCorrienteDelCliente = resp.ListaEntidad ?? [];

                        return Json(new { ok = true, hayDatos = resp.ListaEntidad?.Any() });
                    }
                    else
                    {
                        _logger?.LogInformation($"ℹ️ No se encontraron registros de la cuenta corriente de la cuenta {cta_id}");
                        return Json(new { ok = true, hayDatos = false, mensaje = "No se encontraron registros en la cuenta corriente." });
                    }                               
                }
                else
                {
                    _logger?.LogWarning($"❌ Error al obtener los registros de la cuenta corriente de la cuenta {cta_id}: {resp.Mensaje}");
                    return Json(new { ok = false, mensaje = resp.Mensaje });
                }
            }
            catch (Exception ex)
            {
                string msg = $"❌ Error al obtener los registros de la cuenta corriente de la cuenta {cta_id}";
                _logger?.LogError(ex, msg);
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Esta action tiene por finalidad ser el nexo entre el UI y la api que 
        /// permitira obtener los registros de la cuenta corriente de la cuenta.
        /// 
        /// </summary>
        /// <param name="cta_id">Para obtener los registros es necesario tener la cuenta</param>
        /// <returns>una lista de registros de cuenta corriente </returns>
        [HttpPost]
        public async Task<JsonResult> ObtenerCtaCte()
        {
            try
            {
                var lista = CuentaCorrienteDelCliente;

                return Json(new { ok = true, lista });               
            }
            catch (Exception ex)
            {
                string msg = $"❌ Error al obtener los registros de la cuenta corriente de la cuenta";
                _logger?.LogError(ex, msg);
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult ResguardarCuentaCorrienteSeleccionadaParaElCobro([FromBody] ResguardarCtaCteSeleccionadaRequestDto req)
        {
            try
            {
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📥 RESGUARDAR CUENTA CORRIENTE SELECCIONADA");
                _logger?.LogInformation($"   Usuario: {UserName}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                if (!VerificarAutenticacion(out _))
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "Sesión expirada. Por favor, inicie sesión nuevamente."
                    });
                }

                if (req?.Registros == null)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "La solicitud no posee registros de Cuenta Corriente."
                    });
                }

                if (req.Registros.Count == 0)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "Debe seleccionar al menos un registro para cobrar."
                    });
                }

                var registrosDisponibles = CuentaCorrienteDelCliente ?? new List<CtaCteResponseDto>();
                var registrosSeleccionados = new List<CtaCteResponseDto>();
                var clavesProcesadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


                foreach (var registroSolicitado in req.Registros)
                {
                    if (string.IsNullOrWhiteSpace(registroSolicitado.cta_id) ||
                        string.IsNullOrWhiteSpace(registroSolicitado.tco_id) ||
                        string.IsNullOrWhiteSpace(registroSolicitado.cm_compte) ||
                        string.IsNullOrWhiteSpace(registroSolicitado.ctacte))
                    {
                        return Json(new
                        {
                            ok = false,
                            mensaje = "Uno de los comprobantes seleccionados no posee datos identificatorios completos."
                        });
                    }

                    var clave = string.Join("|",
                        registroSolicitado.cta_id,
                        registroSolicitado.tco_id,
                        registroSolicitado.cm_compte,
                        registroSolicitado.cm_compte_cuota,
                        registroSolicitado.ctacte
                    );

                    if (!clavesProcesadas.Add(clave))
                    {
                        return Json(new
                        {
                            ok = false,
                            mensaje = "Existen comprobantes duplicados en la selección."
                        });
                    }

                    var registroOriginal = registrosDisponibles.FirstOrDefault(x =>
                        x.cta_id == registroSolicitado.cta_id &&
                        x.tco_id == registroSolicitado.tco_id &&
                        x.cm_compte == registroSolicitado.cm_compte &&
                        x.cm_compte_cuota == registroSolicitado.cm_compte_cuota &&
                        x.ctacte == registroSolicitado.ctacte
                    );

                    if (registroOriginal == null)
                    {
                        return Json(new
                        {
                            ok = false,
                            mensaje = $"El comprobante {registroSolicitado.tco_id} {registroSolicitado.cm_compte} ya no está disponible para cobrar."
                        });
                    }

                    if (registroSolicitado.cv_importe <= 0)
                    {
                        return Json(new
                        {
                            ok = false,
                            mensaje = $"El importe del comprobante {registroSolicitado.cm_compte} debe ser mayor a cero."
                        });
                    }

                    // Registro original.cv_importe es el saldo pendiente real
                    // recibido desde el servicio y almacenado en sesión.
                    if (registroSolicitado.cv_importe > registroOriginal.cv_importe)
                    {
                        return Json(new
                        {
                            ok = false,
                            mensaje = $"El importe del comprobante {registroSolicitado.cm_compte} supera el saldo disponible."
                        });
                    }

                    registrosSeleccionados.Add(new CtaCteResponseDto
                    {
                        cta_id = registroOriginal.cta_id,
                        dia_movi = registroOriginal.dia_movi,
                        tco_id = registroOriginal.tco_id,
                        cm_compte = registroOriginal.cm_compte,
                        cm_compte_cuota = registroOriginal.cm_compte_cuota,
                        cv_fecha_vto = registroOriginal.cv_fecha_vto,

                        // Este es el valor que el usuario decidió cobrar.
                        cv_importe = registroSolicitado.cv_importe,

                        // Siempre se conserva el valor histórico del servidor.
                        cv_importe_ori = registroOriginal.cv_importe_ori,

                        cv_concepto = registroOriginal.cv_concepto,
                        ve_id = registroOriginal.ve_id,
                        ccb_id = registroOriginal.ccb_id,
                        ctacte = registroOriginal.ctacte,
                        carga = registroOriginal.carga,
                        carga_obligatoria = registroOriginal.carga_obligatoria
                    });
                }

                CuentaCorrienteDelClienteSeleccionadaParaElCobro =
                    registrosSeleccionados;

                _logger?.LogInformation(
                    "✅ Se resguardaron {Cantidad} registros de Cuenta Corriente. Total: {Total}",
                    registrosSeleccionados.Count,
                    registrosSeleccionados.Sum(x => x.cv_importe)
                );

                return Json(new
                {
                    ok = true,
                    mensaje = "Registros de Cuenta Corriente resguardados correctamente."
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "❌ Error al resguardar los registros seleccionados de Cuenta Corriente."
                );

                return Json(new
                {
                    ok = false,
                    mensaje = "Ocurrió un error inesperado al resguardar la selección."
                });
            }
        }
    }

    public class ResguardarCtaCteSeleccionadaRequestDto
    {
        public List<CtaCteResponseDto> Registros { get; set; } = new();
    }
}
