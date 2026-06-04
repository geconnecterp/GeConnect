using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.caja.core.Servicios.Implementacion.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;

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
            if (BancosLista.Count == 0)
            {
                await ObtenerProveedores(_pagoFactServicio);
            }
        }
        /// <summary>
        /// esta action permitirá obtener la lista de bancos cargadas en sesion
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ObtenerBancos()
        {
            try
            {
                var lista = BancosLista;
                if (lista == null || !lista.Any())
                {
                    lista = [];
                    lista.Add(new ABMChequeListaDto { bc_id = "0000", bc_denominacion = "Sin bancos disponibles", bc_lista = "(default) Sin bancos disponibles" });
                }
                return Json(new { ok = true, bancos = lista });
            }
            catch (NegocioException ex)
            {
                _logger?.LogWarning("⚠️ Error de negocio al obtener bancos: {Mensaje}", ex.Message);
                return Json(new { ok = false, error = false, warn = true, mensaje = ex.Message ?? "Ocurrió un error de negocio al obtener los bancos" });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Excepción al obtener bancos");
                return Json(new { ok = false, error = true, warn = false, mensaje = "Ocurrió un error al obtener los bancos" });
            }
        }

        /// <summary>
        /// ✅ CORREGIDO v20.2.1: Confirmación de compra con valores de pago
        /// CORRECCIÓN CRÍTICA: Usar DTO wrapper para recibir datos del AJAX
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> FinalizarCompra([FromBody] PagoCompletoDto pagoDto)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // ❶ VALIDAR AUTENTICACIÓN
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "Sesión expirada" });

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("⏱️ FINALIZAR COMPRA - INICIO v20.2.1");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❷ ✅ NUEVO: VALIDAR QUE EL DTO NO SEA NULL
                if (pagoDto == null)
                {
                    _logger?.LogError("❌ CRÍTICO: pagoDto es null - El modelo no se deserializó");
                    return Json(new { ok = false, mensaje = "Error: No se recibieron datos del pago" });
                }

                _logger?.LogInformation($"✅ pagoDto recibido: Valores={pagoDto.Valores?.Count ?? 0}, Uniones={pagoDto.Uniones?.Count ?? 0}");

                // ❸ VALIDAR DATOS DE CAJA
                var cajaActual = CajaActual;
                if (cajaActual == null)
                {
                    _logger?.LogError("❌ No hay caja en sesión");
                    return Json(new { ok = false, mensaje = "No hay caja abierta" });
                }

                // ❹ VALIDAR DATOS DE CLIENTE
                var clienteActual = ClienteActual;
                if (clienteActual == null)
                {
                    _logger?.LogError("❌ No hay cliente en sesión");
                    return Json(new { ok = false, mensaje = "No hay cliente seleccionado" });
                }

                // ❺ VALIDAR QUE HAYA PRODUCTOS
                var productosFactura = FacturaProductos;
                if (productosFactura == null || productosFactura.Count == 0)
                {
                    _logger?.LogWarning("❌ No hay productos en la factura");
                    return Json(new { ok = false, mensaje = "Debe cargar al menos un producto" });
                }

                // ❻ VALIDAR QUE HAYA SUBTOTALES
                var subtotalesFactura = FacturaSubtotales;
                if (subtotalesFactura == null || subtotalesFactura.Count == 0)
                {
                    _logger?.LogWarning("❌ No hay subtotales calculados");
                    return Json(new { ok = false, mensaje = "Debe calcular los totales primero" });
                }

                _logger?.LogInformation($"✅ Productos: {productosFactura.Count}");
                _logger?.LogInformation($"✅ Subtotales: {subtotalesFactura.Count}");

                // ❼ ✅ CORREGIDO: VALIDAR QUE HAYA VALORES DE PAGO DESDE EL DTO
                var valores = pagoDto.Valores;
                var uniones = pagoDto.Uniones ?? new List<Json_Union>();

                if (valores == null || valores.Count == 0)
                {
                    _logger?.LogWarning("❌ No se recibieron valores de pago en el DTO");
                    _logger?.LogWarning($"   pagoDto.Valores es null: {valores == null}");
                    _logger?.LogWarning($"   pagoDto.Valores.Count: {valores?.Count ?? 0}");
                    return Json(new { ok = false, mensaje = "Debe especificar al menos un valor de pago" });
                }

                _logger?.LogInformation($"✅ Valores de pago recibidos: {valores.Count}");
                _logger?.LogInformation($"✅ Uniones recibidas: {uniones.Count}");

                // ❽ LOG DETALLADO DE VALORES RECIBIDOS
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📋 VALORES DE PAGO RECIBIDOS:");
                for (int i = 0; i < valores.Count; i++)
                {
                    var valor = valores[i];
                    _logger?.LogInformation($"   [{i + 1}] {valor.ins_id}:");
                    _logger?.LogInformation($"       rb_nro_valor: {valor.rb_nro_valor}");
                    _logger?.LogInformation($"       rb_importe: {valor.rb_importe}");
                    _logger?.LogInformation($"       rb_fecha_valor: {valor.rb_fecha_valor:yyyy-MM-dd}");
                    _logger?.LogInformation($"       rb_dato1_valor: {valor.rb_dato1_valor}");
                    _logger?.LogInformation($"       rb_dato2_valor: {valor.rb_dato2_valor}");
                    _logger?.LogInformation($"       rb_dato3_valor: {valor.rb_dato3_valor}");
                }
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❾ SERIALIZAR JSONs
                string jsonProductos = JsonConvert.SerializeObject(productosFactura);
                string jsonSubtotales = JsonConvert.SerializeObject(subtotalesFactura);

                var sorteosFactura = FacturaSorteos;
                string jsonSorteos = JsonConvert.SerializeObject(sorteosFactura);

                // ❿ SERIALIZAR JSON DE VALORES DE PAGO
                string jsonValores = JsonConvert.SerializeObject(valores);

                _logger?.LogInformation($"✅ JSON productos (longitud): {jsonProductos.Length}");
                _logger?.LogInformation($"✅ JSON subtotales (longitud): {jsonSubtotales.Length}");
                _logger?.LogInformation($"✅ JSON sorteos (longitud): {jsonSorteos.Length}");
                _logger?.LogInformation($"✅ JSON valores (longitud): {jsonValores.Length}");

                // ⓫ DETERMINAR IDENTIFICADOR DEL CLIENTE Y TIPO DE OPERACIÓN
                string ctaId;
                string coTipo;
                string origenUpper = clienteActual.Origen?.ToUpper() ?? "F";

                if (origenUpper == "F") // Consumidor Final
                {
                    ctaId = clienteActual.cta_documento ?? string.Empty;
                    LP_Id = cajaActual.Caja.lp_id_min;
                    coTipo = "CF";

                    _logger?.LogInformation($"✅ Cliente CF → Identificador (documento): {ctaId}");
                    _logger?.LogInformation($"✅ co_tipo: {coTipo}");
                }
                else // Cliente Registrado
                {
                    ctaId = clienteActual.cta_id ?? string.Empty;
                    LP_Id = cajaActual.Caja.lp_id_may;
                    coTipo = "CR";

                    _logger?.LogInformation($"✅ Cliente Registrado → Identificador (cta_id): {ctaId}");
                    _logger?.LogInformation($"✅ co_tipo: {coTipo}");
                }

                jsonProductos = jsonProductos.Replace("\\", "");
                jsonSorteos = jsonSorteos.Replace("\\", "");
                jsonSubtotales = jsonSubtotales.Replace("\\", "");
                jsonValores = jsonValores.Replace("\\", "");

                // ⓬ CONSTRUIR REQUEST DTO
                var request = new CajaOpeConfirmarReq
                {
                    // ═══ Datos de caja ═══
                    caja_id = cajaActual.CajaId ?? string.Empty,
                    usu_id = UserName ?? string.Empty,
                    adm_id = cajaActual.AdmId ?? AdministracionId,
                    lp_id = LP_Id ?? string.Empty,
                    caja_nro_proceso = cajaActual.Caja.caja_nro_proceso ?? string.Empty,
                    caja_nro_cierre = cajaActual.Caja.caja_nro_cierre,

                    // ═══ Datos de cliente ═══
                    cta_id = ctaId,
                    ctac_dto = clienteActual.ctac_dto_operacion,
                    ctc_id = clienteActual.ctc_id ?? string.Empty,

                    // ═══ Tipo de operación DINÁMICO ═══
                    co_tipo = coTipo,

                    // ═══ Datos de comprobante ═══
                    tco_letra = clienteActual.tco_letra ?? string.Empty,
                    tco_id_ori = string.Empty,
                    cm_compte_ori = string.Empty,

                    // ═══ Datos fiscales ═══
                    afip_id = clienteActual.afip_id ?? string.Empty,
                    tdoc_id = clienteActual.tdoc_id ?? string.Empty,
                    cta_documento = clienteActual.cta_documento ?? string.Empty,
                    cta_denominacion = clienteActual.cta_denominacion ?? string.Empty,
                    cta_domicilio = clienteActual.cta_domicilio ?? string.Empty,

                    // ═══ Vendedor (opcional) ═══
                    ve_id = string.Empty,

                    // ═══ JSONs de operación ═══
                    json_p = jsonProductos,
                    json_subtotal = jsonSubtotales,
                    json_sorteo = jsonSorteos,

                    // ═══ JSONs de pago CON VALORES ═══
                    json_valores = jsonValores,
                    json_cancela = "{}",
                    json_union = "{}"
                };

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📦 REQUEST DTO CONSTRUIDO");
                _logger?.LogInformation($"   co_tipo: {request.co_tipo}");
                _logger?.LogInformation($"   cta_id: {request.cta_id}");
                _logger?.LogInformation($"   json_valores (longitud): {request.json_valores.Length}");
                _logger?.LogInformation($"   json_valores: {JsonConvert.SerializeObject(jsonValores)}");
                _logger?.LogInformation($"   Request: {JsonConvert.SerializeObject(request)}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ⓭ INVOCAR SERVICIO
                var token = TokenCookie;
                if (string.IsNullOrEmpty(token))
                {
                    _logger?.LogError("❌ No hay token de autenticación");
                    return Json(new { ok = false, mensaje = "Sesión expirada" });
                }

                _logger?.LogInformation("📡 Invocando servicio PagoFactServicio.FinalizarCompra...");
                var resultado = await _pagoFactServicio.FinalizarCompra(request, token);

                stopwatch.Stop();
                _logger?.LogInformation($"⏱️ Tiempo de ejecución: {stopwatch.ElapsedMilliseconds}ms");

                // ⓮ VALIDAR RESPUESTA
                if (resultado == null)
                {
                    _logger?.LogError("❌ El servicio retornó null");
                    return Json(new { ok = false, mensaje = "Error al procesar el pago" });
                }

                if (!resultado.Ok)
                {
                    _logger?.LogWarning($"⚠️ Error del servicio: {resultado.Mensaje}");
                    return Json(new { ok = false, mensaje = resultado.Mensaje ?? "Error al procesar el pago" });
                }

                // ⓯ EXTRAER DATOS DE RESPUESTA
                var respuestaDto = resultado.Entidad;

                if (respuestaDto == null)
                {
                    _logger?.LogError("❌ No se recibió entidad de respuesta");
                    return Json(new { ok = false, mensaje = "Error: respuesta vacía del servidor" });
                }

                // ⓰ VALIDAR RESULTADO DEL SP
                if (respuestaDto.resultado != 0)
                {
                    _logger?.LogError($"❌ Error del SP: {respuestaDto.resultado_msj}");
                    return Json(new
                    {
                        ok = false,
                        mensaje = respuestaDto.resultado_msj ?? "Error al emitir la factura"
                    });
                }

                // ⓱ PARSEAR JSON DE COMPROBANTE
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🔍 PARSEANDO DATOS DEL COMPROBANTE");
                _logger?.LogInformation($"   resultado_id raw: {respuestaDto.resultado_id}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                if (!TryParsearComprobanteJson(respuestaDto.resultado_id, out var comprobante))
                {
                    _logger?.LogError("❌ No se pudo parsear resultado_id como JSON");

                    return Json(new
                    {
                        ok = false,
                        mensaje = "Error al procesar datos del comprobante. Formato inválido.",
                        debug_resultado_id = respuestaDto.resultado_id
                    });
                }

                if (comprobante == null)
                {
                    _logger?.LogError("❌ Comprobante es null después del parseo");
                    return Json(new { ok = false, mensaje = "Error: no se obtuvieron datos del comprobante" });
                }

                // ⓲ LOGS DE DATOS PARSEADOS
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("✅ FACTURA EMITIDA Y PAGADA EXITOSAMENTE");
                _logger?.LogInformation($"   Letra: {comprobante.tco_letra}");
                _logger?.LogInformation($"   ID Tipo: {comprobante.tco_id}");
                _logger?.LogInformation($"   Número: {comprobante.cm_compte}");
                _logger?.LogInformation($"   Repetido: {(comprobante.EsRepetido ? "SÍ" : "NO")}");
                _logger?.LogInformation($"   Mensaje: {respuestaDto.resultado_msj}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ⓳ LIMPIAR SESIÓN DE FACTURA
                FacturaProductos = new List<ProductoFactJsonDto>();
                FacturaSubtotales = [];
                FacturaSorteos = [];

                _logger?.LogInformation("✅ Sesión de factura limpiada");

                // ⓴ RETORNAR RESPUESTA CORRECTA PARA FRONTEND
                return Json(new
                {
                    ok = true,
                    mensaje = $"Factura {comprobante.tco_letra} Nro {comprobante.cm_compte} emitida y pagada exitosamente",

                    data = new[]
                    {
                new
                {
                    tco_letra = comprobante.tco_letra,
                    tco_id = comprobante.tco_id,
                    cm_compte = comprobante.cm_compte,
                    cm_repetido = comprobante.cm_repetido
                }
            },

                    resultado_completo = respuestaDto.resultado_msj,
                    debe_imprimir = true
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger?.LogError($"❌ EXCEPCIÓN en FinalizarCompra: {ex.Message}");
                _logger?.LogError($"   Stack Trace: {ex.StackTrace}");
                _logger?.LogError($"   Tiempo antes del error: {stopwatch.ElapsedMilliseconds}ms");

                return Json(new
                {
                    ok = false,
                    mensaje = "Error inesperado al procesar el pago. Por favor, intente nuevamente."
                });
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
