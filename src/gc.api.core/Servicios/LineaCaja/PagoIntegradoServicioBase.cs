using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.ReglasCaja;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Response;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
namespace gc.api.core.Servicios.LineaCaja;

public abstract class PagoIntegradoServicioBase : CajaBaseServicio
{
    protected PagoIntegradoServicioBase(IUnitOfWork uow, ILoggerHelper logger) : base(uow, logger) { }
    protected virtual List<T> Ejecutar<T>(string sp, List<SqlParameter> p) where T : class => _repository.EjecutarLstSpExt<T>(sp, p, true);
    protected static void ContextoValido(PagoIntegradoContextoDto c)
    {
        if (string.IsNullOrWhiteSpace(c.adm_id) || c.adm_id.Length > 4 || string.IsNullOrWhiteSpace(c.caja_id) || c.caja_id.Length > 4 || string.IsNullOrWhiteSpace(c.caja_nro_proceso) || c.caja_nro_proceso.Length > 15 || c.caja_nro_cierre < 0 || (c.cta_id?.Length ?? 0) > 10 || !Guid.TryParse(c.operacion_id, out _) || !Regex.IsMatch(c.huella ?? "", "^[A-F0-9]{64}$") || !(new[] { "CR", "CF", "CD", "CC", "CV", "IV" }).Contains(c.co_tipo))
            throw new InvalidOperationException("Falta un contexto válido del puesto, cliente u operación de pago integrado.");
    }
    protected List<T> ObtenerOpciones<T>(PagoIntegradoContextoDto c, string? insId, string medio, string nombre) where T : PagoIntegradoOpcionDto
    {
        ContextoValido(c);
        insId = (insId ?? "").Trim();
        var medios = Ejecutar<ValoresMPResDto>("SPGECO_CAJA_Valores_MP", [new("@co_tipo", c.co_tipo), new("@cta_id", c.cta_id), new("@adm_id", c.adm_id)]);
        if (!medios.Any(m => string.Equals(m.tcf_id?.Trim(), medio, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(nombre + " no está habilitado para esta operación y cliente.");
        var instrumentos = Ejecutar<ValoresInsResDto>("SPGECO_CAJA_Valores_Ins", [new("@tcf_id", medio), new("@co_tipo", c.co_tipo), new("@cta_id", c.cta_id), new("@adm_id", c.adm_id)]);
        var codigos = instrumentos.Select(i => i.ins_id?.Trim() ?? "").Where(id => id.Length is > 0 and <= 3 && id != "DEF").Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (insId.Length == 0)
        {
            if (codigos.Count == 0) return [];
            if (codigos.Count != 1) throw new InvalidOperationException("Hay varios instrumentos " + nombre + " configurados. Revise con el administrador cuál debe utilizar este medio.");
            insId = codigos[0];
        }
        else if (!codigos.Contains(insId, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("El instrumento solicitado no está habilitado para " + nombre + ".");
        var opciones = Ejecutar<T>("SPGECO_CAJA_Valores_Ins_Opciones", [new SqlParameter("@ins_id", SqlDbType.Char, 3) { Value = insId }, new("@co_tipo", c.co_tipo), new("@cta_id", c.cta_id), new("@adm_id", c.adm_id)]);
        return opciones.Where(o => string.Equals(o.ins_id?.Trim(), insId, StringComparison.OrdinalIgnoreCase) && o.cuota > 0 && o.cuota <= short.MaxValue && o.pos_plan >= 0 && o.pos_plan <= short.MaxValue && !string.IsNullOrWhiteSpace(o.pos_desc) && (c.co_tipo != "CD" || o.recargo_porcentaje == 0)).ToList();
    }

    protected static void ValidarImportes(PagoIntegradoPreparacionDto p, PagoIntegradoOpcionDto opcion)
    {
        if (p.monto_base <= 0 || p.monto_base != decimal.Round(p.monto_base, 2) || p.monto_total > 9999999999999.99m || p.monto_total <= 0 || p.recargo != PagosIntegrados.Recargo(p.monto_base, opcion.recargo_porcentaje) || p.monto_total != p.monto_base + p.recargo)
            throw new InvalidOperationException("El importe, total o recargo no coinciden con la opción vigente del servidor.");
    }
    protected static Json_Valor CrearValorBase(PagoIntegradoRespuestaSpDto r, PagoIntegradoPreparacionDto p, decimal porcentaje) => new() {
        rb_nro_valor = "001", ins_id = (r.ins_id ?? "").Trim(), rb_dato1_valor = r.rb_dato1_valor ?? "",
        rb_dato2_valor = r.rb_dato2_valor ?? "", rb_dato3_valor = r.rb_dato3_valor ?? "",
        rb_opcion_cuota = r.rb_opcion_cuota.ToString(CultureInfo.InvariantCulture), rb_fecha_valor = r.rb_fecha_valor,
        rb_importe = r.rb_importe, rb_rec = porcentaje, rb_aux = p.recargo,
        rb_cupon_manual = string.IsNullOrWhiteSpace(p.paymentid) ? "N" : "S", rb_ch_dif = "N", rb_estado = "N", id_externo = r.rb_dato1_valor ?? ""
    };
}
