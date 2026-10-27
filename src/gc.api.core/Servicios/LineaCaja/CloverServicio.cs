using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace gc.api.core.Servicios.LineaCaja;

public class CloverServicio : CajaBaseServicio
{
    public CloverServicio(IUnitOfWork uow, ILoggerHelper logger) : base(uow, logger) { }
    protected virtual List<T> Ejecutar<T>(string sp, List<SqlParameter> p) where T : class => _repository.EjecutarLstSpExt<T>(sp, p, true);
    public static decimal Recargo(decimal monto, decimal porcentaje) => decimal.Round(monto * porcentaje / 100m, 2, MidpointRounding.AwayFromZero);
    private static void ContextoValido(CloverContextoDto c)
    {
        if (string.IsNullOrWhiteSpace(c.adm_id) || c.adm_id.Length > 4 || string.IsNullOrWhiteSpace(c.caja_id) || c.caja_id.Length > 4 || string.IsNullOrWhiteSpace(c.caja_nro_proceso) || c.caja_nro_proceso.Length > 15 || c.caja_nro_cierre < 0 || c.cta_id.Length > 10 || !Guid.TryParse(c.operacion_id, out _) || !Regex.IsMatch(c.huella, "^[A-F0-9]{64}$") || !(new[] { "CR", "CF", "CD", "CC", "CV", "IV" }).Contains(c.co_tipo))
            throw new InvalidOperationException("Falta un contexto válido del puesto, cliente u operación Clover.");
    }
    public List<CloverOpcionDto> Opciones(CloverContextoDto c, string? insId)
    {
        ContextoValido(c);
        insId = (insId ?? "").Trim();
        var medios = Ejecutar<ValoresMPResDto>("SPGECO_CAJA_Valores_MP", [new("@co_tipo", c.co_tipo), new("@cta_id", c.cta_id), new("@adm_id", c.adm_id)]);
        if (!medios.Any(m => string.Equals(m.tcf_id?.Trim(), "T1", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("POS Clover no está habilitado para esta operación y cliente.");
        var instrumentos = Ejecutar<ValoresInsResDto>("SPGECO_CAJA_Valores_Ins", [new("@tcf_id", "T1"), new("@co_tipo", c.co_tipo), new("@cta_id", c.cta_id), new("@adm_id", c.adm_id)]);
        var codigos = instrumentos.Select(i => i.ins_id?.Trim() ?? "").Where(id => id.Length is > 0 and <= 3 && id != "DEF").Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (insId.Length == 0)
        {
            if (codigos.Count == 0) return [];
            if (codigos.Count != 1) throw new InvalidOperationException("Hay varios instrumentos Clover configurados. Revise con el administrador cuál debe utilizar este medio.");
            insId = codigos[0];
        }
        else if (!codigos.Contains(insId, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("El instrumento solicitado no está habilitado para POS Clover.");
        var opciones = Ejecutar<CloverOpcionDto>("SPGECO_CAJA_Valores_Ins_Opciones", [new SqlParameter("@ins_id", SqlDbType.Char, 3) { Value = insId }, new("@co_tipo", c.co_tipo), new("@cta_id", c.cta_id), new("@adm_id", c.adm_id)]);
        return opciones.Where(o => string.Equals(o.ins_id?.Trim(), insId, StringComparison.OrdinalIgnoreCase) && o.cuota > 0 && o.cuota <= short.MaxValue && o.pos_plan >= 0 && o.pos_plan <= short.MaxValue && !string.IsNullOrWhiteSpace(o.pos_desc) && (c.co_tipo != "CD" || o.recargo_porcentaje == 0)).ToList();
    }
    // Cada solicitud representa un medio de pago independiente. El SP decide su resultado;
    // no se consulta ni se valida un estado persistido del POS o de otro detalle de Caja.
    public RespuestaDto Procesar(CloverPeticionDto req)
    {
        var c = req.contexto; var p = req.pago; ContextoValido(c);
        if (!Guid.TryParse(p.intento_id, out _) || string.IsNullOrWhiteSpace(p.ins_id) || p.ins_id.Length > 3 || p.co_tipo != c.co_tipo)
            throw new InvalidOperationException("La solicitud Clover no es válida.");
        var manual = !string.IsNullOrWhiteSpace(p.paymentid);
        p.paymentid = (p.paymentid ?? "").Trim().ToUpperInvariant();
        if (manual && !Regex.IsMatch(p.paymentid, "^[A-Z0-9]{4,30}$"))
            throw new InvalidOperationException("La recuperación requiere entre 4 y 30 caracteres alfanuméricos. El SP actual no admite IDs de tres sin iniciar un cobro.");
        var opcion = Opciones(c, p.ins_id).SingleOrDefault(o => o.pos_plan == p.pos_plan && o.cuota == p.cuotas_reales);
        if (opcion == null) throw new InvalidOperationException("La opción de cuotas no está disponible. Consulte la configuración de Clover con el administrador.");
        if (p.monto_base <= 0 || p.monto_base != decimal.Round(p.monto_base, 2) || p.monto_total > 9999999999999.99m || p.monto_total <= 0 || p.recargo != Recargo(p.monto_base, opcion.recargo_porcentaje) || p.monto_total != p.monto_base + p.recargo)
            throw new InvalidOperationException("El importe, total o recargo no coinciden con la opción vigente del servidor.");
        var parametros = new List<SqlParameter> { new("@adm_id", c.adm_id), new("@caja_id", c.caja_id), new("@caja_nro_proceso", c.caja_nro_proceso), new("@caja_nro_cierre", c.caja_nro_cierre), new("@cta_id", c.cta_id),
            new("@monto", SqlDbType.Decimal) { Precision = 15, Scale = 2, Value = p.monto_total }, new("@cashback", SqlDbType.Decimal) { Precision = 15, Scale = 2, Value = 0m }, new("@cuota", p.pos_plan), new("@qr", p.qr), new("@paymentid", SqlDbType.VarChar, 30) { Value = p.paymentid } };
        _logger.Log(TraceEventType.Information, $"[CLOVER][SP REQUEST] Operacion={c.operacion_id}; Intento={p.intento_id}\n" + JsonConvert.SerializeObject(parametros.ToDictionary(x => x.ParameterName, x => x.Value), Formatting.Indented));
        // Sin reintentos automáticos: una pérdida de respuesta no autoriza otro cobro.
        var filas = Ejecutar<CloverRespuestaSpDto>("SPGECO_CAJA_Clover_Pay", parametros);
        _logger.Log(TraceEventType.Information, "[CLOVER][SP RESPONSE]\n" + JsonConvert.SerializeObject(filas, Formatting.Indented));
        if (filas.Count != 1) throw new InvalidOperationException("El SP no devolvió una única respuesta de pago. Verifique el resultado antes de repetir.");
        var r = filas[0];
        // El SP vigente llama respuesta/ respuesta_msj a estas columnas. Se adapta
        // al contrato tradicional de la API sin modificar el procedimiento SQL.
        return new RespuestaDto {
            resultado = r.respuesta,
            resultado_msj = r.respuesta_msj ?? "",
            resultado_id = r.respuesta == 0 ? JsonConvert.SerializeObject(new[] { CrearValor(r, p, opcion.recargo_porcentaje) }) : ""
        };
    }
    public static Json_Valor CrearValor(CloverRespuestaSpDto r, CloverPreparacionDto p, decimal porcentajeRecargo) => new() {
        rb_nro_valor = "001", ins_id = (r.ins_id ?? "").Trim(), rb_dato1_valor = r.rb_dato1_valor ?? "",
        rb_dato2_valor = r.rb_dato2_valor ?? "", rb_dato3_valor = r.rb_dato3_valor ?? "",
        rb_opcion_cuota = r.rb_opcion_cuota.ToString(CultureInfo.InvariantCulture), rb_fecha_valor = r.rb_fecha_valor,
        rb_importe = r.rb_importe, rb_rec = porcentajeRecargo, rb_cupon_manual = string.IsNullOrWhiteSpace(p.paymentid) ? "N" : "S",
        rb_ch_dif = "N", rb_estado = "N", rb_aux = p.recargo, id_externo = r.rb_dato1_valor ?? "" };
}
