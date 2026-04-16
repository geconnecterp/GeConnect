using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    [Authorize]
    public class ProductoFactController : ControladorBaseCaja
    {
        private readonly ICajaServicio _cajaServicio;
        private readonly IProductoFactServicio _productoFactServicio;

        public ProductoFactController(
            IOptions<AppSettings> options,
            ICajaServicio cajaServicio,
            IProductoFactServicio productoFactServicio, // ✅ INYECTAR
            IHttpContextAccessor httpContext,
            ILogger<ProductoFactController> logger) : base(options, httpContext, logger)
        {
            _cajaServicio = cajaServicio;
            _productoFactServicio = productoFactServicio; // ✅ ASIGNAR
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// ✅ NUEVO: Obtiene los datos de un producto para agregarlo a la factura
        /// </summary>
        /// <param name="tipoValor">Tipo de búsqueda: P (Producto), F (Pre-Factura), C (Cotización)</param>
        /// <param name="valor">Valor según tipo: ID producto/barras, ID prefactura, ID cotización</param>
        /// <param name="cantidad">Cantidad del producto (default: 1)</param>
        /// <param name="bulto">Si la cantidad es por bulto (default: true)</param>
        /// <returns>JSON con datos del producto o error</returns>
        [HttpPost]
        public async Task<IActionResult> ObtenerProductoDatos(
            string tipoValor,
            string valor,
            decimal cantidad = 1,
            bool bulto = true)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🔍 OBTENER DATOS DE PRODUCTO - INICIO");
                _logger?.LogInformation($"   Parámetros recibidos:");
                _logger?.LogInformation($"   - Tipo Valor: {tipoValor}");
                _logger?.LogInformation($"   - Valor: {valor}");
                _logger?.LogInformation($"   - Cantidad: {cantidad}");
                _logger?.LogInformation($"   - Bulto: {bulto}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❶ VALIDAR PARÁMETROS DE ENTRADA
                if (string.IsNullOrWhiteSpace(tipoValor))
                {
                    _logger?.LogWarning("❌ Parámetro 'tipoValor' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar el tipo de búsqueda" });
                }

                if (string.IsNullOrWhiteSpace(valor))
                {
                    _logger?.LogWarning("❌ Parámetro 'valor' es requerido");
                    return Json(new { ok = false, mensaje = "Debe ingresar un código de producto" });
                }

                // Validar tipo de búsqueda
                tipoValor = tipoValor.ToUpper().Trim();
                if (tipoValor != "P" && tipoValor != "F" && tipoValor != "C")
                {
                    _logger?.LogWarning($"❌ Tipo de valor inválido: {tipoValor}");
                    return Json(new { ok = false, mensaje = "Tipo de búsqueda inválido. Use P (Producto), F (Pre-Factura) o C (Cotización)" });
                }

                // Validar cantidad
                if (cantidad <= 0)
                {
                    _logger?.LogWarning($"❌ Cantidad inválida: {cantidad}");
                    return Json(new { ok = false, mensaje = "La cantidad debe ser mayor a cero" });
                }

                // ❷ VALIDAR CLIENTE ACTUAL (REQUERIDO)
                var clienteActual = ClienteActual;
                if (clienteActual == null)
                {
                    _logger?.LogWarning("❌ No hay cliente seleccionado en sesión");
                    return Json(new { ok = false, mensaje = "No hay cliente seleccionado. Por favor, identifique un cliente primero." });
                }

                _logger?.LogInformation("✅ Cliente actual obtenido de sesión:");
                _logger?.LogInformation($"   - Nombre: {clienteActual.cta_denominacion}");
                _logger?.LogInformation($"   - Origen: {clienteActual.Origen} ({clienteActual.valida_desc})");
                _logger?.LogInformation($"   - CTA_ID: {clienteActual.cta_id}");
                _logger?.LogInformation($"   - Documento: {clienteActual.cta_documento}");
                _logger?.LogInformation($"   - CTC_ID: {clienteActual.ctc_id}");
                _logger?.LogInformation($"   - Descuento Operación: {clienteActual.ctac_dto_operacion}");

                // ❸ VALIDAR CAJA ACTUAL (REQUERIDO)
                var cajaActual = CajaActual;
                if (cajaActual == null)
                {
                    _logger?.LogWarning("❌ No hay caja seleccionada en sesión");
                    return Json(new { ok = false, mensaje = "No hay caja abierta. Por favor, inicie sesión de caja." });
                }

                _logger?.LogInformation("✅ Caja actual obtenida de sesión:");
                _logger?.LogInformation($"   - Caja ID: {cajaActual.CajaId}");
                _logger?.LogInformation($"   - Administración: {cajaActual.AdmId}");
                _logger?.LogInformation($"   - Lista Precios (Min): {cajaActual.Caja.lp_id_min}");
                _logger?.LogInformation($"   - Lista Precios (Max): {cajaActual.Caja.lp_id_may}");

                // ❹ DETERMINAR LISTA DE PRECIOS
                string listaPreciosId;
                string origenUpper = clienteActual.Origen?.ToUpper() ?? "F";

                if (origenUpper == "F") // Consumidor Final
                {
                    listaPreciosId = cajaActual.Caja.lp_id_min ?? string.Empty;
                    _logger?.LogInformation($"✅ Cliente Consumidor Final → Lista de Precios MIN: {listaPreciosId}");
                }
                else if (origenUpper == "C") // Cliente Registrado
                {
                    listaPreciosId = cajaActual.Caja.lp_id_may ?? string.Empty;
                    _logger?.LogInformation($"✅ Cliente Registrado → Lista de Precios MAX: {listaPreciosId}");
                }
                else
                {
                    // Fallback: usar mínima
                    listaPreciosId = cajaActual.Caja.lp_id_min ?? string.Empty;
                    _logger?.LogWarning($"⚠️ Origen desconocido '{origenUpper}' → Usando Lista de Precios MIN: {listaPreciosId}");
                }

                if (string.IsNullOrEmpty(listaPreciosId))
                {
                    _logger?.LogError("❌ No se pudo determinar la lista de precios");
                    return Json(new { ok = false, mensaje = "Error de configuración: No hay lista de precios asignada a la caja" });
                }

                // ❺ DETERMINAR CANAL (ctc_id)
                // Usar el canal del cliente si existe, sino determinar por origen
                string canalId = clienteActual.ctc_id ?? string.Empty;
                
                if (string.IsNullOrEmpty(canalId))
                {
                    if (origenUpper == "F")
                    {
                        canalId = "MI"; // Minorista (Consumidor Final)
                        _logger?.LogInformation($"✅ Canal determinado por origen (CF): {canalId}");
                    }
                    else
                    {
                        canalId = "MA"; // Mayorista (Cliente Registrado)
                        _logger?.LogInformation($"✅ Canal determinado por origen (Registrado): {canalId}");
                    }
                }
                else
                {
                    _logger?.LogInformation($"✅ Canal obtenido del cliente: {canalId}");
                }

                // ❻ DETERMINAR IDENTIFICADOR DEL CLIENTE (cta_id)
                // Según origen: F (documento) o C (cta_id)
                string identificadorCliente;
                
                if (origenUpper == "F")
                {
                    identificadorCliente = clienteActual.cta_documento ?? string.Empty;
                    _logger?.LogInformation($"✅ Identificador para CF (documento): {identificadorCliente}");
                }
                else
                {
                    identificadorCliente = clienteActual.cta_id ?? string.Empty;
                    _logger?.LogInformation($"✅ Identificador para Registrado (cta_id): {identificadorCliente}");
                }

                if (string.IsNullOrEmpty(identificadorCliente))
                {
                    _logger?.LogError("❌ No se pudo determinar el identificador del cliente");
                    return Json(new { ok = false, mensaje = "Error: Datos incompletos del cliente" });
                }

                // ❼ CONSTRUIR REQUEST DTO
                var request = new ProductoDatosRequestDto
                {
                    tipo_valor = tipoValor,
                    valor = valor.Trim(),
                    lp_id = listaPreciosId,
                    adm_id = cajaActual.AdmId ?? AdministracionId,
                    cantidad = cantidad,
                    bulto = bulto,
                    ctc_id = canalId,
                    cta_id = identificadorCliente,
                    ctac_dto = clienteActual.ctac_dto_operacion 
                };

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📦 REQUEST DTO CONSTRUIDO:");
                _logger?.LogInformation($"   - tipo_valor: {request.tipo_valor}");
                _logger?.LogInformation($"   - valor: {request.valor}");
                _logger?.LogInformation($"   - lp_id: {request.lp_id}");
                _logger?.LogInformation($"   - adm_id: {request.adm_id}");
                _logger?.LogInformation($"   - cantidad: {request.cantidad}");
                _logger?.LogInformation($"   - bulto: {request.bulto}");
                _logger?.LogInformation($"   - ctc_id: {request.ctc_id}");
                _logger?.LogInformation($"   - cta_id: {request.cta_id}");
                _logger?.LogInformation($"   - ctac_dto: {request.ctac_dto}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❽ INVOCAR SERVICIO
                var token = TokenCookie;
                if (string.IsNullOrEmpty(token))
                {
                    _logger?.LogError("❌ No se pudo obtener el token de autenticación");
                    return Json(new { ok = false, mensaje = "Sesión expirada. Por favor, vuelva a iniciar sesión." });
                }

                _logger?.LogInformation("📡 Invocando servicio ProductoFactServicio.ObtenerProductoDatos...");
                var resultado = await _productoFactServicio.ObtenerProductoDatos(request, token);

                stopwatch.Stop();
                _logger?.LogInformation($"⏱️ Tiempo de ejecución: {stopwatch.ElapsedMilliseconds}ms");

                // ❾ PROCESAR RESPUESTA
                if (resultado == null)
                {
                    _logger?.LogError("❌ El servicio retornó null");
                    return Json(new { ok = false, mensaje = "Error al obtener datos del producto" });
                }

                if (!resultado.Ok)
                {
                    _logger?.LogWarning($"⚠️ Servicio retornó error: {resultado.Mensaje}");
                    return Json(new 
                    { 
                        ok = false, 
                        mensaje = resultado.Mensaje ?? "Error al obtener datos del producto" 
                    });
                }

                // ❿ VALIDAR ENTIDAD DE RESPUESTA
                if (resultado.ListaEntidad == null)
                {
                    _logger?.LogError("❌ La entidad de respuesta es null");
                    return Json(new { ok = false, mensaje = "No se recibieron datos del producto" });
                }

                var productos = resultado.ListaEntidad;

                // ⓫ DETECTAR WARNINGS O ERRORES EN LA RESPUESTA
                if (resultado.EsWarn)
                {
                    _logger?.LogWarning($"⚠️ WARNING del servidor: {resultado.Mensaje}");
                    return Json(new 
                    { 
                        ok = false,
                        esWarning = true,
                        mensaje = resultado.Mensaje,
                        producto = productos
                    });
                }

                if (resultado.EsError)
                {
                    _logger?.LogError($"❌ ERROR del servidor: {resultado.Mensaje}");
                    return Json(new 
                    { 
                        ok = false,
                        esError = true,
                        mensaje = resultado.Mensaje
                    });
                }

                // ⓬ ÉXITO
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("✅ PRODUCTO OBTENIDO EXITOSAMENTE");
                _logger?.LogInformation($"   Código: {productos.respuesta}");
                _logger?.LogInformation($"   Mensaje: {productos.respuesta_msj}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                return Json(new 
                { 
                    ok = true, 
                    mensaje = resultado.Mensaje ?? "Producto cargado correctamente",
                    producto = productos
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger?.LogError($"❌ EXCEPCIÓN en ObtenerProductoDatos: {ex.Message}");
                _logger?.LogError($"   Stack Trace: {ex.StackTrace}");
                _logger?.LogError($"   Tiempo transcurrido antes del error: {stopwatch.ElapsedMilliseconds}ms");
                
                return Json(new 
                { 
                    ok = false, 
                    mensaje = "Error inesperado al procesar el producto. Por favor, intente nuevamente." 
                });
            }
        }
    }
}
