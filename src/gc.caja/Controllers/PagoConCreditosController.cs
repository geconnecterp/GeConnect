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

namespace gc.caja.Controllers;

public abstract class PagoConCreditosController : ControladorBaseCaja
{
    protected readonly ICheckoutServicio _pagoFactServicio;
    protected PagoConCreditosController(IOptions<AppSettings> options, ICheckoutServicio pago,
        IHttpContextAccessor context, ILogger logger) : base(options, context, logger) { _pagoFactServicio = pago; }
        #region Helpers de validación NC

        protected static bool PermiteNotasCredito(string coTipo)
        {
            return string.Equals(coTipo, "CF", StringComparison.OrdinalIgnoreCase)
                || string.Equals(coTipo, "CR", StringComparison.OrdinalIgnoreCase)
                || string.Equals(coTipo, "CC", StringComparison.OrdinalIgnoreCase)
                || string.Equals(coTipo, "CD", StringComparison.OrdinalIgnoreCase);
        }

        protected sealed class ValidacionUnionesNcResult
        {
            public bool Ok { get; init; }

            public string Mensaje { get; init; } = string.Empty;

            public List<Json_Union> Uniones { get; init; } = [];

            public decimal TotalImputado { get; init; }
        }

        protected static string NormalizarClaveNc(string? valor)
        {
            return (valor ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
        }

        protected static bool TieneIdentidadNc(Json_Union union)
        {
            return !string.IsNullOrWhiteSpace(union.cta_id) &&
                   !string.IsNullOrWhiteSpace(union.tco_id) &&
                   !string.IsNullOrWhiteSpace(union.cm_compte) &&
                   !string.IsNullOrWhiteSpace(union.cm_compte_cuota);
        }

        protected static bool TieneIdentidadNcVigente(ValoresNCResDto credito, string coTipo)
        {
            // El segundo SELECT del SP devuelve devoluciones CD sin dia_movi.
            var esDevolucionDiferida = coTipo == "CD" &&
                NormalizarClaveNc(credito.ctacte) == "N" &&
                NormalizarClaveNc(credito.carga) == "S" &&
                NormalizarClaveNc(credito.carga_obligatoria) == "S";

            return TieneIdentidadNc(credito) &&
                (!string.IsNullOrWhiteSpace(credito.dia_movi) || esDevolucionDiferida);
        }

        protected static string CrearClaveNc(Json_Union union)
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

        protected static bool TryParseImporteNc(
            string? valor,
            out decimal importe)
        {
            importe = 0m;

            if (string.IsNullOrWhiteSpace(valor))
            {
                return false;
            }

            var texto = valor.Trim().Replace(',', '.');

            // Los importes del contrato no llevan separadores de miles.
            var estilos =
                NumberStyles.AllowLeadingSign |
                NumberStyles.AllowDecimalPoint;

            return decimal.TryParse(
                       texto,
                       estilos,
                       CultureInfo.InvariantCulture,
                       out importe);
        }

        protected static string FormatearImporteNcJson(decimal importe)
        {
            return importe.ToString(
                "0.00",
                CultureInfo.InvariantCulture
            );
        }

        protected static Json_Union CrearUnionNcCanonica(
            ValoresNCResDto creditoVigente,
            decimal importeImputado,
            decimal importeOriginal,
            DateTime fechaVencimiento)
        {
            return new Json_Union
            {
                cta_id = creditoVigente.cta_id,
                dia_movi = creditoVigente.dia_movi,
                tco_id = creditoVigente.tco_id,
                cm_compte = creditoVigente.cm_compte,
                cm_compte_cuota = creditoVigente.cm_compte_cuota,
                cv_fecha_vto = fechaVencimiento.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture),
                cv_fecha_carga = creditoVigente.cv_fecha_carga,

                // Crédito aplicado, con signo contable negativo.
                cv_importe = FormatearImporteNcJson(
                    -Math.Abs(importeImputado)
                ),

                // Crédito original informado por SP.
                cv_importe_ori = FormatearImporteNcJson(importeOriginal),

                cv_concepto = creditoVigente.cv_concepto,
                ve_id = creditoVigente.ve_id,
                ccb_id = creditoVigente.ccb_id
            };
        }

        protected static bool TryParseFechaNc(string? valor, out DateTime fecha)
        {
            // El DTO legado transporta fechas SQL como texto regional o ISO, sin zona horaria.
            string[] formatos =
            [
                "d/M/yyyy H:mm:ss", "d/M/yyyy H:mm:ss.FFFFFFF", "d/M/yyyy",
                "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
                "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.FFFFFFF", "yyyy-MM-dd"
            ];
            return DateTime.TryParseExact(valor?.Trim(), formatos,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);
        }

        protected static decimal ObtenerTotalValoresConvencionales(
    IEnumerable<Json_Valor> valores)
        {
            return (valores ?? Enumerable.Empty<Json_Valor>())
                .Sum(x => x.rb_importe);
        }

        protected static decimal ObtenerTotalOperacionParaNc(
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

        protected static bool ValidarNcContraSaldoPendiente(
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

        protected async Task<ValidacionUnionesNcResult>
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
                if (!TieneIdentidadNcVigente(credito, coTipoNormalizado))
                {
                    _logger?.LogError(
                        "[NC] Crédito sin identidad completa. CoTipo={CoTipo}, Cuenta={Cuenta}, Movimiento={Movimiento}, Tipo={Tipo}, Comprobante={Comprobante}, Cuota={Cuota}, Ctacte={Ctacte}, Carga={Carga}, Obligatoria={Obligatoria}",
                        coTipoNormalizado, credito.cta_id, credito.dia_movi,
                        credito.tco_id, credito.cm_compte, credito.cm_compte_cuota,
                        credito.ctacte, credito.carga, credito.carga_obligatoria
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

                if (!creditoVigente.cv_fecha_carga.HasValue)
                {
                    _logger?.LogError("[NC] Falta cv_fecha_carga en SPGECO_CAJA_Valores_NC. Clave={Clave}", clave);
                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje = $"El crédito {creditoVigente.cm_compte} no tiene fecha de carga. Recargue la operación; si persiste, revise la respuesta de Valores NC."
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

                var textoImporteOriginal = string.IsNullOrWhiteSpace(creditoVigente.cv_importe_ori)
                    ? creditoVigente.cv_importe
                    : creditoVigente.cv_importe_ori;

                if (!TryParseImporteNc(textoImporteOriginal, out var importeOriginal) ||
                    !TryParseFechaNc(creditoVigente.cv_fecha_vto, out var fechaVencimiento))
                {
                    _logger?.LogError(
                        "[NC] Datos no convertibles al contrato SQL. Clave={Clave}, ImporteOriginal={ImporteOriginal}, FechaVencimiento={FechaVencimiento}",
                        clave, textoImporteOriginal, creditoVigente.cv_fecha_vto);
                    return new ValidacionUnionesNcResult
                    {
                        Ok = false,
                        Mensaje = $"El crédito {creditoVigente.cm_compte} tiene un importe original o vencimiento inválido. Revise la respuesta de Valores NC."
                    };
                }

                unionesCanonicas.Add(
                    CrearUnionNcCanonica(
                        creditoVigente,
                        importeImputado,
                        importeOriginal,
                        fechaVencimiento
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
}
