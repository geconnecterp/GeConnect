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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class CheckoutController : ControladorBaseCaja
    {
        private readonly ICheckoutServicio _pagoFactServicio;
        private readonly ICajaServicio _cajaServicio; // ✅ NUEVO v21.0

        public CheckoutController(IOptions<AppSettings> options,
            ICheckoutServicio pagoFactServicio,
            ICajaServicio cajaServicio, // ✅ NUEVO v21.0
            IHttpContextAccessor httpContext,
            ILogger<CheckoutController> logger) : base(options, httpContext, logger)
        {
            _pagoFactServicio = pagoFactServicio;
            _cajaServicio = cajaServicio; // ✅ NUEVO v21.0
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

        #region Helpers de validación NC

        private static bool PermiteNotasCredito(string coTipo)
        {
            return string.Equals(coTipo, "CF", StringComparison.OrdinalIgnoreCase)
                || string.Equals(coTipo, "CR", StringComparison.OrdinalIgnoreCase)
                || string.Equals(coTipo, "CC", StringComparison.OrdinalIgnoreCase)
                || string.Equals(coTipo, "CD", StringComparison.OrdinalIgnoreCase);
        }

        private sealed class ValidacionUnionesNcResult
        {
            public bool Ok { get; init; }

            public string Mensaje { get; init; } = string.Empty;

            public List<Json_Union> Uniones { get; init; } = [];

            public decimal TotalImputado { get; init; }
        }

        private static string NormalizarClaveNc(string? valor)
        {
            return (valor ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
        }

        private static bool TieneIdentidadNc(Json_Union union)
        {
            return !string.IsNullOrWhiteSpace(union.cta_id) &&
                   !string.IsNullOrWhiteSpace(union.dia_movi) &&
                   !string.IsNullOrWhiteSpace(union.tco_id) &&
                   !string.IsNullOrWhiteSpace(union.cm_compte) &&
                   !string.IsNullOrWhiteSpace(union.cm_compte_cuota);
        }

        private static string CrearClaveNc(Json_Union union)
        {
            return string.Join("|", new[]
            {
        NormalizarClaveNc(union.cta_id),
        NormalizarClaveNc(union.dia_movi),
        NormalizarClaveNc(union.tco_id),
        NormalizarClaveNc(union.cm_compte),
        NormalizarClaveNc(union.cm_compte_cuota),
        NormalizarClaveNc(union.ve_id),
        NormalizarClaveNc(union.ccb_id)
    });
        }

        private static bool TryParseImporteNc(
            string? valor,
            out decimal importe)
        {
            importe = 0m;

            if (string.IsNullOrWhiteSpace(valor))
            {
                return false;
            }

            var texto = valor.Trim();

            var estilos =
                NumberStyles.AllowLeadingSign |
                NumberStyles.AllowDecimalPoint |
                NumberStyles.AllowThousands;

            return decimal.TryParse(
                       texto,
                       estilos,
                       CultureInfo.InvariantCulture,
                       out importe)
                   ||
                   decimal.TryParse(
                       texto,
                       estilos,
                       CultureInfo.GetCultureInfo("es-AR"),
                       out importe);
        }

        private static string FormatearImporteNcJson(decimal importe)
        {
            return importe.ToString(
                "0.00",
                CultureInfo.InvariantCulture
            );
        }

        private static Json_Union CrearUnionNcCanonica(
            ValoresNCResDto creditoVigente,
            decimal importeImputado)
        {
            var importeOriginal = !string.IsNullOrWhiteSpace(
                creditoVigente.cv_importe_ori
            )
                ? creditoVigente.cv_importe_ori
                : creditoVigente.cv_importe;

            return new Json_Union
            {
                cta_id = creditoVigente.cta_id,
                dia_movi = creditoVigente.dia_movi,
                tco_id = creditoVigente.tco_id,
                cm_compte = creditoVigente.cm_compte,
                cm_compte_cuota = creditoVigente.cm_compte_cuota,
                cv_fecha_vto = creditoVigente.cv_fecha_vto,

                // Crédito aplicado, con signo contable negativo.
                cv_importe = FormatearImporteNcJson(
                    -Math.Abs(importeImputado)
                ),

                // Crédito original informado por SP.
                cv_importe_ori = importeOriginal,

                cv_concepto = creditoVigente.cv_concepto,
                ve_id = creditoVigente.ve_id,
                ccb_id = creditoVigente.ccb_id
            };
        }

        private static decimal ObtenerTotalValoresConvencionales(
    IEnumerable<Json_Valor> valores)
        {
            return (valores ?? Enumerable.Empty<Json_Valor>())
                .Sum(x => x.rb_importe);
        }

        private static decimal ObtenerTotalOperacionParaNc(
            bool esCobranzaGeneral,
            decimal importeCobranza,
            IEnumerable<FactSubtotalJsonDto> subtotales)
        {
            if (esCobranzaGeneral)
            {
                return importeCobranza;
            }

            // En Facturación, FacturaSubtotales debe contener importes
            // con signo ya aplicado: subtotal + recargos - descuentos.
            return (subtotales ?? Enumerable.Empty<FactSubtotalJsonDto>())
                .Sum(x => x.importe);
        }

        private static bool ValidarNcContraSaldoPendiente(
            decimal totalOperacion,
            decimal totalValoresConvencionales,
            decimal totalNc,
            out string mensaje)
        {
            mensaje = string.Empty;

            if (totalNc <= 0m)
            {
                return true;
            }

            if (totalOperacion <= 0m)
            {
                mensaje =
                    "No se pudo determinar un total válido para aplicar Notas de Crédito.";

                return false;
            }

            if (totalValoresConvencionales < 0m)
            {
                mensaje =
                    "Los medios de pago convencionales contienen un importe inválido.";

                return false;
            }

            var saldoPendiente = totalOperacion - totalValoresConvencionales;

            if (saldoPendiente < 0m)
            {
                saldoPendiente = 0m;
            }

            if (totalNc > saldoPendiente + 0.01m)
            {
                mensaje =
                    "El total de Notas de Crédito supera el saldo pendiente de la operación.";

                return false;
            }

            return true;
        }

        private async Task<ValidacionUnionesNcResult>
            ValidarYConstruirUnionesNcAsync(
                List<Json_Union>? unionesSolicitadas,
                string coTipo,
                bool esConsumidorFinal)
        {
            var solicitadas = unionesSolicitadas ?? [];

            var coTipoNormalizado = (coTipo ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (!PermiteNotasCredito(coTipoNormalizado))
            {
                // El módulo actual puede finalizar normalmente,
                // pero no admite NC hasta implementar su regla específica.
                if (solicitadas.Count == 0)
                {
                    return new ValidacionUnionesNcResult
                    {
                        Ok = true,
                        Uniones = [],
                        TotalImputado = 0m
                    };
                }

                return new ValidacionUnionesNcResult
                {
                    Ok = false,
                    Mensaje =
                        "El módulo actual no admite Notas de Crédito como forma de pago."
                };
            }

            var cliente = ClienteActual;

            if (cliente == null)
            {
                return new ValidacionUnionesNcResult
                {
                    Ok = false,
                    Mensaje =
                        "No hay cliente seleccionado para validar los créditos."
                };
            }

            var cuentaCliente = string.Equals(
                cliente.Origen,
                "C",
                StringComparison.OrdinalIgnoreCase
            )
                ? cliente.cta_id
                : cliente.cta_documento;

            if (string.IsNullOrWhiteSpace(cuentaCliente))
            {
                if (solicitadas.Count == 0)
                {
                    return new ValidacionUnionesNcResult
                    {
                        Ok = true,
                        Uniones = [],
                        TotalImputado = 0m
                    };
                }

                return new ValidacionUnionesNcResult
                {
                    Ok = false,
                    Mensaje =
                        "No se pudo identificar la cuenta del cliente para validar los créditos."
                };
            }

            if (string.IsNullOrWhiteSpace(AdministracionId))
            {
                return new ValidacionUnionesNcResult
                {
                    Ok = false,
                    Mensaje =
                        "No se encontró la administración activa para validar los créditos."
                };
            }

            var requestNc = new ValoresNCReqDto
            {
                co_tipo = coTipoNormalizado,
                cta_id = cuentaCliente,
                adm_id = AdministracionId
            };

            var respuestaNc = await _pagoFactServicio.ObtenerValoresNC(
                requestNc,
                TokenCookie
            );

            if (respuestaNc == null)
            {
                return new ValidacionUnionesNcResult
                {
                    Ok = false,
                    Mensaje =
                        "No fue posible validar los créditos disponibles."
                };
            }

            if (!respuestaNc.Ok)
            {
                return new ValidacionUnionesNcResult
                {
                    Ok = false,
                    Mensaje =
                        respuestaNc.Mensaje ??
                        "No fue posible validar los créditos disponibles."
                };
            }

            var creditosVigentes = respuestaNc.ListaEntidad ?? [];

            var creditosPorClave = new Dictionary<
                string,
                ValoresNCResDto
            >(StringComparer.OrdinalIgnoreCase);

            foreach (var credito in creditosVigentes)
            {
                if (!TieneIdentidadNc(credito))
                {
                    _logger?.LogError(
                        "[NC] SP devolvió un crédito sin identidad completa."
                    );

                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje =
                            "Se recibió un crédito inválido desde el servidor."
                    };
                }

                var clave = CrearClaveNc(credito);

                if (!creditosPorClave.TryAdd(clave, credito))
                {
                    _logger?.LogError(
                        "[NC] El SP devolvió créditos duplicados. Clave={Clave}",
                        clave
                    );

                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje =
                            "Se detectaron créditos duplicados. Recargue la operación."
                    };
                }
            }

            var clavesSolicitadas = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

            var unionesCanonicas = new List<Json_Union>();
            decimal totalImputado = 0m;

            foreach (var unionSolicitada in solicitadas)
            {
                if (!TieneIdentidadNc(unionSolicitada))
                {
                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje =
                            "Uno de los créditos seleccionados no posee identidad completa."
                    };
                }

                var clave = CrearClaveNc(unionSolicitada);

                if (!clavesSolicitadas.Add(clave))
                {
                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje =
                            "No se puede utilizar el mismo crédito más de una vez."
                    };
                }

                if (!creditosPorClave.TryGetValue(
                    clave,
                    out var creditoVigente
                ))
                {
                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje =
                            "Uno de los créditos seleccionados ya no está disponible. Recargue la operación."
                    };
                }

                if (!TryParseImporteNc(
                    unionSolicitada.cv_importe,
                    out var importeSolicitado
                ))
                {
                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje =
                            "Uno de los créditos posee un importe inválido."
                    };
                }

                var importeImputado = Math.Abs(importeSolicitado);

                if (importeImputado <= 0m)
                {
                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje =
                            "El importe imputado de un crédito debe ser mayor a cero."
                    };
                }

                if (!TryParseImporteNc(
                    creditoVigente.cv_importe,
                    out var importeVigente
                ))
                {
                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje =
                            "No se pudo interpretar el saldo de uno de los créditos."
                    };
                }

                var saldoDisponible = Math.Abs(importeVigente);

                if (importeImputado > saldoDisponible + 0.01m)
                {
                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje =
                            "El importe imputado supera el saldo disponible de un crédito."
                    };
                }

                var esObligatorio = string.Equals(
                    creditoVigente.carga_obligatoria,
                    "S",
                    StringComparison.OrdinalIgnoreCase
                );

                if (esConsumidorFinal && !esObligatorio)
                {
                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje =
                            "Un Consumidor Final no puede utilizar créditos opcionales desde este módulo."
                    };
                }

                if (
                    esObligatorio &&
                    Math.Abs(importeImputado - saldoDisponible) > 0.01m
                )
                {
                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje =
                            "Los créditos obligatorios deben aplicarse por su importe total."
                    };
                }

                unionesCanonicas.Add(
                    CrearUnionNcCanonica(
                        creditoVigente,
                        importeImputado
                    )
                );

                totalImputado += importeImputado;
            }

            // El backend exige que todas las NC obligatorias vigentes estén presentes.
            foreach (var creditoVigente in creditosVigentes)
            {
                var esObligatorio = string.Equals(
                    creditoVigente.carga_obligatoria,
                    "S",
                    StringComparison.OrdinalIgnoreCase
                );

                if (!esObligatorio)
                {
                    continue;
                }

                if (!TryParseImporteNc(
                    creditoVigente.cv_importe,
                    out var importeVigente
                ))
                {
                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje =
                            "No se pudo interpretar el importe de un crédito obligatorio."
                    };
                }

                if (Math.Abs(importeVigente) <= 0m)
                {
                    continue;
                }

                var clave = CrearClaveNc(creditoVigente);

                if (!clavesSolicitadas.Contains(clave))
                {
                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje =
                            "Existe un crédito obligatorio que no fue imputado."
                    };
                }
            }

            _logger?.LogInformation(
                "[NC] Validación exitosa. CoTipo={CoTipo}, Cantidad={Cantidad}, Total={Total}",
                coTipoNormalizado,
                unionesCanonicas.Count,
                totalImputado
            );

            return new ValidacionUnionesNcResult
            {
                Ok = true,
                Uniones = unionesCanonicas,
                TotalImputado = totalImputado
            };
        }
        #endregion

        /// <summary>
        /// ✅ CORREGIDO v20.2.1: Confirmación de compra con valores de pago
        /// CORRECCIÓN CRÍTICA: Usar DTO wrapper para recibir datos del AJAX
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> FinalizarCompra([FromBody] PagoCompletoDto pagoDto)
        {
            var stopwatch = Stopwatch.StartNew();
            List<FactPendienteResponseDto> facts = [];
            List<CtaCteResponseDto> ctaCtes = [];
            List<Json_Cancela>? obligacionACancelar = [];
            string coTipo = string.Empty;

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

                // ✅ PASO 1: SANEAR EL OBJETO DTO COMPLETO CON UNA SOLA LLAMADA
                SanitizarObjeto(pagoDto);

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
                bool esCobranzaDiferidaTemporal = false;
                bool esCobranzaCtaCteTemporal = false;
                bool esCobranzaGen = false;
                decimal importe = 0.00m;
                //!string.IsNullOrEmpty(pagoDto.ModuloOrigen) &&
                //                               pagoDto.ModuloOrigen.ToUpper() == "COBRANZADIFERIDA";

                #region Normalizar ModuloOrigen
                var moduloOrigen = (pagoDto.ModuloOrigen ?? "Facturacion")
                            .Trim()
                            .ToUpperInvariant();

                var modulosPermitidos = new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                )
                    {
                        "FACTURACION",
                        "COBRANZADIFERIDA",
                        "CUENTACORRIENTE"
                    };

                if (!modulosPermitidos.Contains(moduloOrigen))
                {
                    _logger?.LogWarning(
                        "ModuloOrigen inválido: {ModuloOrigen}",
                        moduloOrigen
                    );

                    return Json(new
                    {
                        ok = false,
                        mensaje = "El módulo de origen de la operación no es válido."
                    });
                }
                #endregion

                switch (moduloOrigen)
                {
                    case "COBRANZADIFERIDA":
                        esCobranzaDiferidaTemporal = true;

                        facts = FacturasSeleccionadasParaCobro;
                        // En modo Cobranza Diferida, se deberá generará FacturaSubtotales dinámicamente
                        //[{"orden":1,"tipo":"SU","concepto":"Subtotal","base":0.00,"alicuota":0.00,"importe":JUAN PONE EL IMPROTE TOTAL A CANCELAR,"id_aux":""}]
                        if (facts != null && facts.Count > 0)
                        {
                            importe = facts.Sum(f => f.cv_importe);
                        }
                        FacturaSubtotales = new List<FactSubtotalJsonDto> { new FactSubtotalJsonDto { orden = 1 ,
                            tipo = "SU",
                            concepto = "Subtotal", @base = 0.00m, alicuota = 0.00m, importe = importe, id_aux = "" } };

                        if (facts != null && facts.Count > 0)
                        {
                            _logger?.LogInformation($"✅ {facts.Count} factura(s) obtenidas de SESIÓN");

                            // Convertir de FactPendienteResponseDto a Json_Cancela
                            try
                            {
                                obligacionACancelar = ConvertirFacturasPendientesACancela(facts);

                                _logger?.LogInformation($"   Conversión exitosa: {obligacionACancelar.Count} factura(s) convertidas");

                                // Loguear primeras 3 facturas (muestra)
                                for (int i = 0; i < Math.Min(3, obligacionACancelar.Count); i++)
                                {
                                    var f = obligacionACancelar[i];
                                    _logger?.LogInformation($"   [{i + 1}] {f.tco_id} {f.cm_compte} (Cuota: {f.cm_compte_cuota})");
                                }

                                if (obligacionACancelar.Count > 3)
                                {
                                    _logger?.LogInformation($"   ... y {obligacionACancelar.Count - 3} factura(s) más");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError(ex, "❌ Error al convertir facturas de sesión");
                                obligacionACancelar = null;
                            }
                        }
                        break;
                    case "CUENTACORRIENTE":
                        esCobranzaCtaCteTemporal = true;

                        ctaCtes = CuentaCorrienteDelClienteSeleccionadaParaElCobro;

                        if (ctaCtes != null && ctaCtes.Count > 0)
                        {
                            importe = ctaCtes.Sum(c => c.cv_importe);
                        }

                        FacturaSubtotales = new List<FactSubtotalJsonDto> {
                            new ()
                                {
                                    orden = 1 ,
                                    tipo = "SU",
                                    concepto = "Subtotal",
                                    @base = 0.00m,
                                    alicuota = 0.00m,
                                    importe = importe,
                                    id_aux = ""
                                }
                        };

                        if (ctaCtes != null && ctaCtes.Count > 0)
                        {
                            _logger?.LogInformation($"✅ {ctaCtes.Count} factura(s) obtenidas de SESIÓN");

                            // Convertir de FactPendienteResponseDto a Json_Cancela
                            try
                            {
                                obligacionACancelar = ConvertirObligacionCtaCteACancela(ctaCtes);

                                _logger?.LogInformation($"   Conversión exitosa: {obligacionACancelar.Count} factura(s) convertidas");

                                // Loguear primeras 3 facturas (muestra)
                                for (int i = 0; i < Math.Min(3, obligacionACancelar.Count); i++)
                                {
                                    var f = obligacionACancelar[i];
                                    _logger?.LogInformation($"   [{i + 1}] {f.tco_id} {f.cm_compte} (Cuota: {f.cm_compte_cuota})");
                                }

                                if (obligacionACancelar.Count > 3)
                                {
                                    _logger?.LogInformation($"   ... y {obligacionACancelar.Count - 3} factura(s) más");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError(ex, "❌ Error al convertir facturas de sesión");
                                obligacionACancelar = null;
                            }
                        }
                        break;
                    default:

                        break;
                }

                //determinamos si es cobranza o no
                esCobranzaGen = esCobranzaCtaCteTemporal || esCobranzaDiferidaTemporal;

                if (!esCobranzaGen && (productosFactura == null || productosFactura.Count == 0))
                {
                    _logger?.LogWarning("❌ No hay productos en la factura (modo Facturación)");
                    return Json(new { ok = false, mensaje = "Debe cargar al menos un producto" });
                }

                // ❻ ✅ CORREGIDO v28.1: VALIDAR SUBTOTALES SOLO SI NO ES COBRANZA DIFERIDA
                var subtotalesFactura = FacturaSubtotales;
                if (!esCobranzaGen && (subtotalesFactura == null || subtotalesFactura.Count == 0))
                {
                    _logger?.LogWarning("❌ No hay subtotales calculados (modo Facturación)");
                    return Json(new { ok = false, mensaje = "Debe calcular los totales primero" });
                }

                // ✅ NUEVO: Log informativo para Cobranza Diferida - 
                // si es cobranza diferida tendremos que regenerar el subtotal pues debe viajar. 
                if (esCobranzaGen)
                {
                    _logger?.LogInformation("✅ Validaciones de Cobranza Diferida o Cuenta Corriente");
                }

                _logger?.LogInformation($"✅ Productos: {productosFactura.Count}");
                _logger?.LogInformation($"✅ Subtotales: {subtotalesFactura.Count}");

                // ❼ ✅ CORREGIDO: VALIDAR QUE HAYA VALORES DE PAGO DESDE EL DTO

                var valores = pagoDto.Valores?.ToList() ?? [];

                var unionesSolicitadas = pagoDto.Uniones?.ToList() ?? [];

                // Cuenta Corriente todavía no utiliza NC.
                // Mantiene exactamente su regla actual: el total de valores
                // convencionales debe coincidir con la deuda seleccionada.
                if (esCobranzaCtaCteTemporal)
                {
                    var totalSeleccionado = ctaCtes?.Sum(x => x.cv_importe) ?? 0m;

                    var totalValoresConvencionales =
                        ObtenerTotalValoresConvencionales(valores);

                    if (Math.Abs(totalSeleccionado - totalValoresConvencionales) > 0.01m)
                    {
                        _logger?.LogWarning(
                            "Monto inconsistente en Cuenta Corriente. Selección={Seleccion}. Valores={Valores}",
                            totalSeleccionado,
                            totalValoresConvencionales
                        );

                        return Json(new
                        {
                            ok = false,
                            mensaje =
                                "El total de los medios de pago no coincide con el importe seleccionado de Cuenta Corriente."
                        });
                    }
                }

                _logger?.LogInformation(
                    "Valores convencionales recibidos: {CantidadValores}",
                    valores.Count
                );

                _logger?.LogInformation(
                    "Uniones NC solicitadas: {CantidadUniones}",
                    unionesSolicitadas.Count
                );

                //var valores = pagoDto.Valores;
                //var unionesSolicitadas = pagoDto.Uniones;

                ////Esto protege el flujo si alguien altera el DOM o el request manualmente.
                //if (esCobranzaCtaCteTemporal)
                //{
                //    var totalSeleccionado = ctaCtes?.Sum(x => x.cv_importe) ?? 0m;

                //    var totalValores = valores?.Sum(x => x.rb_importe) ?? 0m;

                //    if (Math.Abs(totalSeleccionado - totalValores) > 0.01m)
                //    {
                //        _logger?.LogWarning(
                //            "Monto inconsistente en Cuenta Corriente. Selección: {Seleccion}. Valores: {Valores}",
                //            totalSeleccionado,
                //            totalValores
                //        );

                //        return Json(new
                //        {
                //            ok = false,
                //            mensaje = "El total de los medios de pago no coincide con el importe seleccionado de Cuenta Corriente."
                //        });
                //    }
                //}

                //var uniones = pagoDto.Uniones ?? new List<Json_Union>();

                //if (valores == null || valores.Count == 0)
                //{
                //    _logger?.LogWarning("❌ No se recibieron valores de pago en el DTO");
                //    _logger?.LogWarning($"   pagoDto.Valores es null: {valores == null}");
                //    _logger?.LogWarning($"   pagoDto.Valores.Count: {valores?.Count ?? 0}");
                //    return Json(new { ok = false, mensaje = "Debe especificar al menos un valor de pago" });
                //}

                _logger?.LogInformation($"✅ Valores de pago recibidos: {valores.Count}");
                //_logger?.LogInformation($"✅ Uniones recibidas: {uniones.Count}");

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
                //string jsonProductos = JsonConvert.SerializeObject(productosFactura);
                //string jsonSubtotales = JsonConvert.SerializeObject(subtotalesFactura);

                //var sorteosFactura = FacturaSorteos;
                //string jsonSorteos = JsonConvert.SerializeObject(sorteosFactura);

                //// ❿ SERIALIZAR JSON DE VALORES DE PAGO
                //string jsonValores = JsonConvert.SerializeObject(valores);

                //string jsonUniones = JsonConvert.SerializeObject(
                //        uniones,
                //        Formatting.None,
                //        JsonSettings
                //    );

                //_logger?.LogInformation($"✅ JSON productos (longitud): {jsonProductos.Length}");
                //_logger?.LogInformation($"✅ JSON subtotales (longitud): {jsonSubtotales.Length}");
                //_logger?.LogInformation($"✅ JSON sorteos (longitud): {jsonSorteos.Length}");
                //_logger?.LogInformation($"✅ JSON valores (longitud): {jsonValores.Length}");
                //_logger?.LogInformation($"✅ JSON uniones (longitud): {jsonUniones.Length}");

                // ⓫ DETERMINAR IDENTIFICADOR DEL CLIENTE Y TIPO DE OPERACIÓN
                string jsonCancela = "[]";
                string ctaId;
                bool esCobranzaDiferida = esCobranzaDiferidaTemporal;


                // ✅ PASO 1: Detectar módulo origen
                //if (!string.IsNullOrEmpty(pagoDto.ModuloOrigen) &&  pagoDto.ModuloOrigen.ToUpper() == "COBRANZADIFERIDA")
                if (esCobranzaGen)
                {


                    _logger?.LogInformation("═══════════════════════════════════════════════════");
                    _logger?.LogInformation("🔄 MÓDULO DE COBRANZA DETECTADO: {ModuloOrigen}", moduloOrigen);
                    _logger?.LogInformation("═══════════════════════════════════════════════════");

                    // ═══ LÓGICA ESPECÍFICA PARA COBRANZA DIFERIDA ═══
                    switch (moduloOrigen)
                    {
                        case "COBRANZADIFERIDA":
                            coTipo = "CD";
                            break;
                        case "CUENTACORRIENTE":
                            coTipo = "CC";
                            break;
                        default:
                            break;
                    }

                    ctaId = clienteActual.cta_id ?? string.Empty;

                    // ═══════════════════════════════════════════════════════════
                    // ✅ NUEVO v28.1: PRIORIZAR FACTURAS DESDE PAYLOAD
                    // ═══════════════════════════════════════════════════════════

                    _logger?.LogInformation("═══════════════════════════════════════════════════");
                    _logger?.LogInformation("🔍 OBTENIENDO FACTURAS A CANCELAR v28.1");
                    _logger?.LogInformation("═══════════════════════════════════════════════════");

                    // ═══════════════════════════════════════════════════════════
                    // VALIDACIÓN FINAL: Debe haber al menos una factura
                    // ═══════════════════════════════════════════════════════════

                    if (obligacionACancelar == null || obligacionACancelar.Count == 0)
                    {
                        _logger?.LogError("═══════════════════════════════════════════════════");
                        _logger?.LogError("❌ NO HAY FACTURAS SELECCIONADAS PARA COBRANZA DIFERIDA");
                        _logger?.LogError("═══════════════════════════════════════════════════");
                        _logger?.LogError("   Fuentes verificadas:");
                        _logger?.LogError("      1. Payload (pagoDto.Cancelar): Vacío o NULL");
                        _logger?.LogError("      2. Sesión (FacturasSeleccionadasParaCobro): Vacío o NULL");
                        _logger?.LogError("═══════════════════════════════════════════════════");
                        _logger?.LogError("   Diagnóstico adicional:");
                        _logger?.LogError($"      - pagoDto es NULL: {pagoDto == null}");
                        _logger?.LogError($"      - pagoDto.Cancelar es NULL: {pagoDto?.Cancelar == null}");
                        _logger?.LogError($"      - pagoDto.Cancelar.Count: {pagoDto?.Cancelar?.Count ?? 0}");
                        _logger?.LogError($"      - FacturasSeleccionadasParaCobro es NULL: {FacturasSeleccionadasParaCobro == null}");
                        _logger?.LogError($"      - FacturasSeleccionadasParaCobro.Count: {FacturasSeleccionadasParaCobro?.Count ?? 0}");
                        _logger?.LogError("═══════════════════════════════════════════════════");

                        return Json(new
                        {
                            ok = false,
                            mensaje = "Debe seleccionar al menos una obligación (Factura/Cuenta Corriente) para cobrar"
                        });
                    }

                    _logger?.LogInformation("═══════════════════════════════════════════════════");
                    _logger?.LogInformation($"✅ TOTAL FACTURAS A PROCESAR: {obligacionACancelar.Count}");
                    _logger?.LogInformation("═══════════════════════════════════════════════════");

                    // ✅ SERIALIZAR json_cancela
                    jsonCancela = JsonConvert.SerializeObject(obligacionACancelar, Formatting.None, JsonSettings);

                    _logger?.LogInformation($"✅ json_cancela generado:");
                    _logger?.LogInformation($"   Longitud: {jsonCancela.Length} caracteres");
                    _logger?.LogInformation($"   Json_Cancela: {jsonCancela}");
                    _logger?.LogInformation($"   Facturas incluidas: {obligacionACancelar.Count}");

                    // ✅ VALIDACIÓN: En CobranzaDiferida NO debe haber productos nuevos en factura
                    if (productosFactura != null && productosFactura.Count > 0)
                    {
                        _logger?.LogWarning(
                            "Cobranza Diferida detectó productos de sesión. Se omitirán."
                        );

                        productosFactura = [];
                    }

                    // ✅ VALIDACIÓN: CobranzaDiferida debe tener valores de pago
                    //if (valores == null || valores.Count == 0)
                    //{
                    //    _logger?.LogError("❌ CobranzaDiferida requiere valores de pago");
                    //    return Json(new { ok = false, mensaje = "Debe especificar los valores de pago para el cobro" });
                    //}

                    _logger?.LogInformation("═══════════════════════════════════════════════════");
                }
                else
                {
                    // ═══ LÓGICA ORIGINAL PARA FACTURACIÓN NORMAL ═══

                    _logger?.LogInformation("═══════════════════════════════════════════════════");
                    _logger?.LogInformation("📄 MÓDULO: FACTURACIÓN NORMAL");
                    _logger?.LogInformation("═══════════════════════════════════════════════════");
                    string origenUpper = clienteActual.Origen?.ToUpper() ?? "F";

                    if (origenUpper == "F") // Consumidor Final
                    {
                        ctaId = string.Empty;
                        coTipo = "CF";

                        _logger?.LogInformation($"✅ Cliente CF → Identificador (documento): {ctaId}");
                        _logger?.LogInformation($"✅ co_tipo: {coTipo}");
                    }
                    else // Cliente Registrado
                    {
                        ctaId = clienteActual.cta_id ?? string.Empty;
                        coTipo = "CR";

                        _logger?.LogInformation($"✅ Cliente Registrado → Identificador (cta_id): {ctaId}");
                        _logger?.LogInformation($"✅ co_tipo: {coTipo}");
                    }

                    if (string.IsNullOrWhiteSpace(LP_Id))
                    {
                        return Json(new { ok = false, mensaje = "No hay una lista de precios activa para la operación." });
                    }
                }

                var esConsumidorFinal = string.Equals(
                            clienteActual.Origen,
                            "F",
                            StringComparison.OrdinalIgnoreCase
                        );

                // Reconsulta SPGECO_CAJA_Valores_NC y construye las uniones canónicas.
                // Nunca se serializan directamente las uniones que llegaron del navegador.
                var validacionNc = await ValidarYConstruirUnionesNcAsync(
                    unionesSolicitadas,
                    coTipo,
                    esConsumidorFinal
                );

                if (!validacionNc.Ok)
                {
                    _logger?.LogWarning(
                        "[NC] Operación rechazada. Motivo: {Mensaje}",
                        validacionNc.Mensaje
                    );

                    return Json(new
                    {
                        ok = false,
                        mensaje = validacionNc.Mensaje
                    });
                }

                // Única declaración de "uniones" dentro de FinalizarCompra.
                var uniones = validacionNc.Uniones;

                if (valores.Count == 0 && uniones.Count == 0)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje =
                            "Debe especificar al menos un medio de pago o una Nota de Crédito aplicable."
                    });
                }

                // La validación de saldo pendiente solo afecta operaciones
                // que efectivamente utilizan NC.
                if (uniones.Count > 0)
                {
                    var totalOperacion = ObtenerTotalOperacionParaNc(
                        esCobranzaGen,
                        importe,
                        subtotalesFactura
                    );

                    var totalValoresConvencionales =
                        ObtenerTotalValoresConvencionales(valores);

                    if (!ValidarNcContraSaldoPendiente(
                        totalOperacion,
                        totalValoresConvencionales,
                        validacionNc.TotalImputado,
                        out var mensajeNcSaldo))
                    {
                        _logger?.LogWarning(
                            "[NC] Importe inválido. Operación={Operacion}, Valores={Valores}, NC={Nc}. Motivo={Motivo}",
                            totalOperacion,
                            totalValoresConvencionales,
                            validacionNc.TotalImputado,
                            mensajeNcSaldo
                        );

                        return Json(new
                        {
                            ok = false,
                            mensaje = mensajeNcSaldo
                        });
                    }
                }

                // Serializar solamente después de validar contexto, NC y totales.
                var sorteosFactura = FacturaSorteos ?? [];

                string jsonProductos = JsonConvert.SerializeObject(
                    productosFactura ?? [],
                    Formatting.None,
                    JsonSettings
                );

                string jsonSubtotales = JsonConvert.SerializeObject(
                    subtotalesFactura ?? [],
                    Formatting.None,
                    JsonSettings
                );

                string jsonSorteos = JsonConvert.SerializeObject(
                    sorteosFactura,
                    Formatting.None,
                    JsonSettings
                );

                string jsonValores = JsonConvert.SerializeObject(
                    valores,
                    Formatting.None,
                    JsonSettings
                );

                string jsonUniones = JsonConvert.SerializeObject(
                    uniones,
                    Formatting.None,
                    JsonSettings
                );

                _logger?.LogInformation(
                    "[PAGO] JSON final construido. Valores={Valores}, NC={Uniones}, Cancela={Cancela}",
                    valores.Count,
                    uniones.Count,
                    obligacionACancelar?.Count ?? 0
                );


                //jsonProductos = jsonProductos.Replace("\\", "");
                //jsonSorteos = jsonSorteos.Replace("\\", "");
                //jsonSubtotales = jsonSubtotales.Replace("\\", "");
                //jsonValores = jsonValores.Replace("\\", "");
                //jsonUniones = jsonUniones.Replace("\\", "");

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
                    json_cancela = jsonCancela, // ✅ ACTUALIZADO v22.0: Ahora dinámico
                    json_union = jsonUniones,

                };



                // ═══════════════════════════════════════════════════════════
                // ⓭ ✅ NUEVO v21.0: VALIDACIÓN DE ESTADO DEL PUNTO DE VENTA
                // ═══════════════════════════════════════════════════════════

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🔍 VALIDANDO ESTADO DEL PUNTO DE VENTA ANTES DE FINALIZAR COMPRA");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                var validacionPV = ValidarEstadoPuntoVenta(
                    cajaServicio: _cajaServicio,
                    cajaId: cajaActual.CajaId ?? string.Empty,
                    ctrlId: cajaActual.Caja.ctrl_id ?? string.Empty,
                    nroProceso: request.caja_nro_proceso,
                    nroCierre: request.caja_nro_cierre,
                    tipoLlamada: "F" // ✅ "F" = Finalización (emite comprobante)
                ).GetAwaiter().GetResult();

                // ⓮ EVALUAR RESULTADO DE VALIDACIÓN

                // CASO 1: Error bloqueante - NO puede continuar
                if (!validacionPV.PuedeContinuar)
                {
                    _logger?.LogError("❌ Validación de PV falló - Operación bloqueada");
                    _logger?.LogError($"   Resultado: {validacionPV.Resultado}");
                    _logger?.LogError($"   Mensaje: {validacionPV.Mensaje}");

                    stopwatch.Stop();
                    _logger?.LogInformation($"⏱️ Tiempo antes del bloqueo: {stopwatch.ElapsedMilliseconds}ms");

                    return Json(new
                    {
                        ok = false,
                        mensaje = validacionPV.Mensaje,
                        error_tipo = "estado_pv",
                        ctrl_id = validacionPV.CtrlId,
                        resultado_pv = validacionPV.Resultado
                    });
                }

                // CASO 2: Advertencia - Puede continuar pero registrar mensaje
                if (validacionPV.EsAdvertencia)
                {
                    _logger?.LogWarning("⚠️ Validación de PV con advertencia - Operación continúa");
                    _logger?.LogWarning($"   Resultado: {validacionPV.Resultado}");
                    _logger?.LogWarning($"   Mensaje: {validacionPV.Mensaje}");
                }
                else
                {
                    _logger?.LogInformation("✅ Validación de PV exitosa - Operación autorizada");
                }

                // ⓭ INVOCAR SERVICIO
                var token = TokenCookie;
                if (string.IsNullOrEmpty(token))
                {
                    _logger?.LogError("❌ No hay token de autenticación");
                    return Json(new { ok = false, mensaje = "Sesión expirada" });
                }

                //analizamos si el CAEA se activa o no para esta operación
                request.caea = cajaActual.Caja.ctrl_id == "-1" && validacionPV.Resultado == 1 ? true : false;

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📦 REQUEST DTO CONSTRUIDO");
                _logger?.LogInformation($"   co_tipo: {request.co_tipo}");
                _logger?.LogInformation($"   cta_id: {request.cta_id}");

                //estas 2 lineas siguientes deberian ser comentadas luego en producción, ya que muestran datos sensibles del cliente
                _logger?.LogInformation($"   json_valores (longitud): {request.json_valores.Length}");
                _logger?.LogInformation($"   json_valores: {JsonConvert.SerializeObject(jsonValores)}");

                _logger?.LogInformation($"   FormaPago: {cajaActual.Caja.ctrl_id} - Resultado: {validacionPV.Resultado} - CAEA: {request.caea}");
                _logger?.LogInformation("   json_valores: {CantidadValores} valor(es)", valores.Count);

                _logger?.LogInformation("   json_union: {CantidadUniones} NC(s), total imputado: {TotalNc}", uniones.Count, validacionNc.TotalImputado
                );

                _logger?.LogInformation("   json_cancela: {CantidadCancelaciones} obligación(es)", obligacionACancelar?.Count ?? 0
                );

                _logger?.LogInformation("   FormaPago: {CtrlId} - ResultadoPV: {ResultadoPV} - CAEA: {Caea}", cajaActual.Caja.ctrl_id, validacionPV.Resultado,
                    request.caea
                );

                _logger?.LogInformation($"   Request PAGO: {JsonConvert.SerializeObject(request)}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

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

                // ═══════════════════════════════════════════════════════════
                // ✅ NUEVO: REGISTRACIÓN DE STOCK INDEPENDIENTE
                // ═══════════════════════════════════════════════════════════
                try
                {
                    _logger?.LogInformation("🔄 Iniciando actualización de stock para la factura...");
                    string depoId = cajaActual.Caja.depo_id;

                    if (string.IsNullOrEmpty(depoId))
                    {
                        _logger?.LogWarning("⚠️ No se encontró 'depo_id' en la caja actual. No se puede actualizar el stock.");
                    }
                    else
                    {
                        if (string.Equals(
                            moduloOrigen,
                            "FACTURACION",
                            StringComparison.OrdinalIgnoreCase
                        ))
                        {
                            // Construir el ID de comprobante para el stock
                            string stockId = $"{comprobante.tco_id}{comprobante.cm_compte}{comprobante.cm_repetido}";

                            var stockRequest = new CargaStkDto
                            {
                                box_id = depoId,
                                tipo = "FV",
                                id = stockId
                            };

                            _logger?.LogInformation($"   Parámetros de stock: box_id='{stockRequest.box_id}', tipo='{stockRequest.tipo}', id='{stockRequest.id}'");

                            // Usamos el _cajaServicio ya inyectado
                            var stockResult = await _cajaServicio.CargaStkDeFactura(stockRequest, token);

                            if (stockResult == null || !stockResult.Ok)
                            {
                                _logger?.LogError($"❌ Error al actualizar el stock: {stockResult?.Mensaje ?? "Respuesta nula del servicio."}");
                                // NO se retorna error al cliente, el proceso principal fue exitoso.
                            }
                            else
                            {
                                _logger?.LogInformation($"✅ Stock actualizado exitosamente. Comprobante ID:{stockId}");
                            }
                        }
                        else
                        {
                            _logger?.LogInformation(
                                "Actualización de stock omitida. Módulo={ModuloOrigen}",
                                moduloOrigen
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "❌ EXCEPCIÓN CRÍTICA en la actualización de stock. La factura se emitió pero el stock no se actualizó.");
                    // La excepción se captura y registra, pero no se propaga para no afectar la respuesta al cliente.
                }
                // ═══════════════════════════════════════════════════════════

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

                // ✅ NUEVO v22.0: Limpiar facturas seleccionadas si es CobranzaDiferida
                if (esCobranzaDiferida)
                {
                    FacturasSeleccionadasParaCobro = new List<FactPendienteResponseDto>();
                    _logger?.LogInformation("✅ Sesión de FacturasSeleccionadasParaCobro limpiada");
                }

                if (esCobranzaCtaCteTemporal)
                {
                    CuentaCorrienteDelClienteSeleccionadaParaElCobro =
                        new List<CtaCteResponseDto>();

                    _logger?.LogInformation(
                        "✅ Sesión CuentaCorrienteDelClienteSeleccionadaParaElCobro limpiada"
                    );
                }

                _logger?.LogInformation("✅ Sesión de factura limpiada");

                // ⓴ RETORNAR RESPUESTA CORRECTA PARA FRONTEND
                var mensajeExito = esCobranzaDiferida
                    ? $"Cobro de facturas procesado exitosamente. Recibo {comprobante.tco_letra} Nro {comprobante.cm_compte}"
                    : esCobranzaCtaCteTemporal
                        ? $"Cobro de Cuenta Corriente procesado exitosamente. Recibo {comprobante.tco_letra} Nro {comprobante.cm_compte}"
                        : $"Factura {comprobante.tco_letra} Nro {comprobante.cm_compte} emitida y pagada exitosamente";

                var respuestaFinal = new
                {
                    ok = true,
                    mensaje = mensajeExito,

                    data = new[]
                    {
                        new
                        {
                            tco_letra = comprobante.tco_letra,
                            tco_id = comprobante.tco_id,
                            cm_compte = comprobante.cm_compte,
                            cm_repetido = comprobante.cm_repetido,

                            modulo_origen = moduloOrigen,
                            es_cobranza_diferida = esCobranzaDiferida,
                            es_cobranza_cuenta_corriente = esCobranzaCtaCteTemporal,

                            cantidad_nc_aplicadas = uniones.Count,
                            total_nc_imputado = validacionNc.TotalImputado
                        }
                    },

                    resultado_completo = respuestaDto.resultado_msj,
                    debe_imprimir = true
                };

                // ✅ NUEVO v21.0: Agregar advertencia del PV si existe
                if (validacionPV.EsAdvertencia)
                {
                    return Json(new
                    {
                        respuestaFinal.ok,
                        respuestaFinal.mensaje,
                        respuestaFinal.data,
                        respuestaFinal.resultado_completo,
                        respuestaFinal.debe_imprimir,
                        mensaje_advertencia = validacionPV.Mensaje,
                        mostrar_mensaje_pv = true
                    });
                }

                return Json(respuestaFinal);


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

        private List<Json_Cancela> ConvertirObligacionCtaCteACancela(List<CtaCteResponseDto> ctactes)
        {
            if (ctactes == null || ctactes.Count == 0)
            {
                _logger?.LogWarning("⚠️ ConvertirFacturasPendientesACancela: Lista de facturas vacía o nula");
                return new List<Json_Cancela>();
            }

            var resultado = new List<Json_Cancela>();

            _logger?.LogInformation("═══════════════════════════════════════════════════");
            _logger?.LogInformation($"🔄 INICIANDO CONVERSIÓN DE FACTURAS PENDIENTES");
            _logger?.LogInformation($"   Total facturas a convertir: {ctactes.Count}");
            _logger?.LogInformation("═══════════════════════════════════════════════════");

            foreach (var cc in ctactes)
            {
                try
                {
                    // ✅ VALIDACIÓN: Solo procesar facturas seleccionadas
                    //if (!cc.seleccionado)
                    //{
                    //    _logger?.LogInformation($"⏭️ Factura {cc.tco_id}-{cc.cm_compte} NO seleccionada - omitida");
                    //    continue;
                    //}

                    var cancela = new Json_Cancela
                    {
                        cta_id = cc.cta_id,

                        // ✅ Conversión DateTime? → string? (formato ISO esperado por SP)
                        dia_movi = cc.dia_movi,

                        tco_id = cc.tco_id,
                        cm_compte = cc.cm_compte,

                        // ✅ Conversión segura int → short? (validación de rango)
                        cm_compte_cuota = cc.cm_compte_cuota >= short.MinValue && cc.cm_compte_cuota <= short.MaxValue
                            ? (short?)cc.cm_compte_cuota
                            : null,

                        cv_fecha_vto = cc.cv_fecha_vto,
                        cv_importe = cc.cv_importe,
                        cv_importe_ori = cc.cv_importe_ori,

                        // ✅ Valores por defecto para campos no disponibles en origen
                        cv_estado = "A", // A = Aplicado/Activo
                        cv_fecha_carga = DateTime.Now,

                        cv_concepto = cc.cv_concepto,

                        // ✅ Conversión int? → string?
                        ve_id = cc.ve_id?.ToString() ?? string.Empty,
                        ccb_id = cc.ccb_id?.ToString() ?? string.Empty
                    };

                    resultado.Add(cancela);

                    _logger?.LogInformation($"   ✅ [{resultado.Count}] {cancela.tco_id}-{cancela.cm_compte}: ${cancela.cv_importe:N2}");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"❌ Error al convertir factura: tco_id={cc.tco_id}, cm_compte={cc.cm_compte}");
                    // ✅ ROBUSTEZ: Continuar con las demás facturas
                }
            }

            _logger?.LogInformation("═══════════════════════════════════════════════════");
            _logger?.LogInformation($"✅ CONVERSIÓN COMPLETADA: {resultado.Count} facturas procesadas");
            _logger?.LogInformation("═══════════════════════════════════════════════════");

            return resultado;
        }

        /// <summary>
        /// ✅ NUEVO v22.0: Convierte facturas pendientes a DTO de cancelación para json_cancela
        /// Transforma List<FactPendienteResponseDto> → List<CancelaJsonDto>
        /// </summary>
        /// <param name="facturas">Lista de facturas seleccionadas para cobro</param>
        /// <returns>Lista de DTOs formateados para el campo json_cancela del SP</returns>
        private List<Json_Cancela> ConvertirFacturasPendientesACancela(List<FactPendienteResponseDto> facturas)
        {
            if (facturas == null || facturas.Count == 0)
            {
                _logger?.LogWarning("⚠️ ConvertirFacturasPendientesACancela: Lista de facturas vacía o nula");
                return new List<Json_Cancela>();
            }

            var resultado = new List<Json_Cancela>();

            _logger?.LogInformation("═══════════════════════════════════════════════════");
            _logger?.LogInformation($"🔄 INICIANDO CONVERSIÓN DE FACTURAS PENDIENTES");
            _logger?.LogInformation($"   Total facturas a convertir: {facturas.Count}");
            _logger?.LogInformation("═══════════════════════════════════════════════════");

            foreach (var factura in facturas)
            {
                try
                {
                    // ✅ VALIDACIÓN: Solo procesar facturas seleccionadas
                    if (!factura.seleccionado)
                    {
                        _logger?.LogInformation($"⏭️ Factura {factura.tco_id}-{factura.cm_compte} NO seleccionada - omitida");
                        continue;
                    }

                    var cancela = new Json_Cancela
                    {
                        cta_id = factura.cta_id,

                        // ✅ Conversión DateTime? → string? (formato ISO esperado por SP)
                        dia_movi = factura.dia_movi,

                        tco_id = factura.tco_id,
                        cm_compte = factura.cm_compte,

                        // ✅ Conversión segura int → short? (validación de rango)
                        cm_compte_cuota = factura.cm_compte_cuota >= short.MinValue && factura.cm_compte_cuota <= short.MaxValue
                            ? (short?)factura.cm_compte_cuota
                            : null,

                        cv_fecha_vto = factura.cv_fecha_vto,
                        cv_importe = factura.cv_importe,
                        cv_importe_ori = factura.cv_importe_ori,

                        // ✅ Valores por defecto para campos no disponibles en origen
                        cv_estado = "A", // A = Aplicado/Activo
                        cv_fecha_carga = DateTime.Now,

                        cv_concepto = factura.cv_concepto,

                        // ✅ Conversión int? → string?
                        ve_id = factura.ve_id?.ToString() ?? string.Empty,
                        ccb_id = factura.ccb_id?.ToString() ?? string.Empty
                    };

                    resultado.Add(cancela);

                    _logger?.LogInformation($"   ✅ [{resultado.Count}] {cancela.tco_id}-{cancela.cm_compte}: ${cancela.cv_importe:N2}");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"❌ Error al convertir factura: tco_id={factura.tco_id}, cm_compte={factura.cm_compte}");
                    // ✅ ROBUSTEZ: Continuar con las demás facturas
                }
            }

            _logger?.LogInformation("═══════════════════════════════════════════════════");
            _logger?.LogInformation($"✅ CONVERSIÓN COMPLETADA: {resultado.Count} facturas procesadas");
            _logger?.LogInformation("═══════════════════════════════════════════════════");

            return resultado;
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
        public async Task<IActionResult> ObtenerValoresNC([FromBody] ValoresNCReqDto? req)
        {
            try
            {
                if (!VerificarAutenticacion(out _))
                {
                    return Json(new
                    {
                        ok = false,
                        error = true,
                        warn = false,
                        mensaje = "La sesión ha expirado."
                    });
                }

                if (req == null)
                {
                    _logger?.LogWarning("[ObtenerValoresNC] Request vacío.");

                    return Json(new
                    {
                        ok = false,
                        error = true,
                        warn = false,
                        mensaje = "Debe especificar el tipo de operación."
                    });
                }

                var coTipo = (req.co_tipo ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();

                // Alcance definido para NC:
                // CF = Facturación Consumidor Final
                // CR = Facturación Cliente Registrado
                // CD = Cobranza Diferida
                var coTiposPermitidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                        {
                                            "CF",
                                            "CR",
                                            "CD",
                                            "CC"
                                        };

                if (!coTiposPermitidos.Contains(coTipo))
                {
                    _logger?.LogWarning(
                        "[ObtenerValoresNC] co_tipo no permitido para NC: {CoTipo}",
                        coTipo
                    );

                    return Json(new
                    {
                        ok = false,
                        error = true,
                        warn = false,
                        mensaje = "El contexto actual no permite consultar créditos de cuenta corriente."
                    });
                }

                var cliente = ClienteActual;

                if (cliente == null)
                {
                    _logger?.LogWarning("[ObtenerValoresNC] No hay cliente en sesión.");

                    return Json(new
                    {
                        ok = false,
                        error = true,
                        warn = false,
                        mensaje = "Debe seleccionar un cliente antes de consultar créditos."
                    });
                }

                var esConsumidorFinal = string.Equals(
                    cliente.Origen,
                    "F",
                    StringComparison.OrdinalIgnoreCase
                );

                // Para cliente registrado se usa cta_id.
                // Para consumidor final se conserva el criterio actual del sistema:
                // usar cta_documento si existe.
                var cuentaCliente = string.Equals(
                    cliente.Origen,
                    "C",
                    StringComparison.OrdinalIgnoreCase
                )
                    ? cliente.cta_id
                    : cliente.cta_documento;

                // Un consumidor final sin identificador no puede tener créditos
                // consultables contra el SP. No debe romper el pago normal.
                if (string.IsNullOrWhiteSpace(cuentaCliente))
                {
                    if (esConsumidorFinal)
                    {
                        return Json(new
                        {
                            ok = true,
                            error = false,
                            warn = false,

                            sinCreditos = true,
                            codigo = "SIN_CREDITOS_NC",

                            mensaje = "El consumidor final no posee una cuenta identificable para consultar créditos.",
                            esConsumidorFinal = true,

                            datos = Array.Empty<ValoresNCResDto>()
                        });
                    }

                    _logger?.LogWarning(
                        "[ObtenerValoresNC] Cliente sin cta_id ni documento. Origen: {Origen}",
                        cliente.Origen
                    );

                    return Json(new
                    {
                        ok = false,
                        error = true,
                        warn = false,
                        mensaje = "El cliente seleccionado no posee una cuenta válida para consultar créditos."
                    });
                }

                if (string.IsNullOrWhiteSpace(AdministracionId))
                {
                    _logger?.LogError(
                        "[ObtenerValoresNC] AdministracionId no disponible en sesión."
                    );

                    return Json(new
                    {
                        ok = false,
                        error = true,
                        warn = false,
                        mensaje = "Los datos de sesión están incompletos. Recargue la operación."
                    });
                }

                // Nunca confiar en cta_id ni adm_id recibidos desde el navegador.
                req.co_tipo = coTipo;
                req.cta_id = cuentaCliente;
                req.adm_id = AdministracionId;

                _logger?.LogInformation(
                    "[ObtenerValoresNC] Usuario={Usuario}, Adm={AdmId}, Cuenta={Cuenta}, CoTipo={CoTipo}, CF={EsCF}",
                    UserName,
                    req.adm_id,
                    req.cta_id,
                    req.co_tipo,
                    esConsumidorFinal
                );

                var res = await _pagoFactServicio.ObtenerValoresNC(req, TokenCookie);

                if (res == null)
                {
                    _logger?.LogError(
                        "[ObtenerValoresNC] El servicio devolvió null. Cuenta={Cuenta}, CoTipo={CoTipo}",
                        req.cta_id,
                        req.co_tipo
                    );

                    return Json(new
                    {
                        ok = false,
                        error = true,
                        warn = false,
                        mensaje = "No fue posible consultar los créditos disponibles."
                    });
                }

                if (!res.Ok)
                {
                    _logger?.LogWarning(
                        "[ObtenerValoresNC] Servicio respondió error. EsError={EsError}, EsWarn={EsWarn}, Mensaje={Mensaje}",
                        res.EsError,
                        res.EsWarn,
                        res.Mensaje
                    );

                    return Json(new
                    {
                        ok = false,
                        error = res.EsError,
                        warn = res.EsWarn || !res.EsError,
                        mensaje = res.Mensaje ?? "No fue posible consultar los créditos disponibles."
                    });
                }

                var creditos = res.ListaEntidad ?? [];
                var sinCreditos = creditos.Count == 0;

                _logger?.LogInformation(
                    "[ObtenerValoresNC] Resultado={Resultado}. Cantidad={Cantidad}. Cuenta={Cuenta}. CoTipo={CoTipo}",
                    sinCreditos ? "SIN_CREDITOS" : "CON_CREDITOS",
                    creditos.Count,
                    req.cta_id,
                    req.co_tipo
                );

                return Json(new
                {
                    ok = true,
                    error = false,
                    warn = false,

                    sinCreditos,

                    codigo = sinCreditos
                        ? "SIN_CREDITOS_NC"
                        : "CON_CREDITOS_NC",

                    mensaje = sinCreditos
                        ? "Cliente no posee créditos disponibles."
                        : "Créditos disponibles obtenidos correctamente.",

                    esConsumidorFinal,

                    datos = creditos
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[ObtenerValoresNC] Excepción no controlada.");

                return Json(new
                {
                    ok = false,
                    error = true,
                    warn = false,
                    mensaje = "Ocurrió un error al consultar los créditos disponibles."
                });
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
