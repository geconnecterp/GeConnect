using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Response;

namespace gc.caja.core.Servicios.Implementacion.Cajas;

/// <summary>Contrato del instrumento simulado DOC, compartido por los módulos de pago.</summary>
public static class DocumentoCuentaCorriente
{
    public const string TipoMedioPago = "DO";

    public static bool EsMedioDocumento(string? tipo) =>
        string.Equals(tipo?.Trim(), TipoMedioPago, StringComparison.OrdinalIgnoreCase);

    public static bool EstaHabilitado(IEnumerable<ValoresMPResDto>? medios) =>
        medios?.Any(m => EsMedioDocumento(m.tcf_id)) == true;

    public static ValoresInsResDto CrearInstrumentoSimulado() => new()
    {
        ins_id = "DOC", ins_desc = "Documento en Cuenta Corriente", tcf_id = TipoMedioPago,
        ins_detalle = "S", ins_tiene_vto = "S", ins_vigente = "S",
        ins_arqueo = "N", ins_vuelto = "N",
        ins_comision = 0, ins_comision_fija = 0,
        ins_ret_gan = 0, ins_ret_ib = 0, ins_ret_iva = 0
    };

    public static bool EsDocumento(Json_Valor valor) =>
        string.Equals(valor.ins_id?.Trim(), "DOC", StringComparison.OrdinalIgnoreCase);

    public static string? ValidarYNormalizar(IReadOnlyCollection<Json_Valor> valores,
        decimal totalOperacion, decimal totalNc)
    {
        var documentos = valores.Where(EsDocumento).ToList();
        if (documentos.Count == 0) return null;
        var hoy = DateTime.Today;

        foreach (var documento in documentos)
        {
            if (documento.rb_importe <= 0 || decimal.Round(documento.rb_importe, 2) != documento.rb_importe)
                return "Documento en Cuenta Corriente: ingrese un monto mayor a cero con hasta dos decimales.";
            if (documento.rb_fecha_valor.Date == DateTime.MinValue.Date)
                return "Documento en Cuenta Corriente: debe indicar una fecha de vencimiento válida.";
            if (documento.rb_fecha_valor.Date < hoy)
                return "Documento en Cuenta Corriente: la fecha de vencimiento no puede ser anterior a la fecha actual.";
        }

        // DOC no genera vuelto. Incluye los demás valores y las NC validadas por el servidor.
        if (totalNc < 0 || valores.Any(v => v.rb_importe < 0) ||
            valores.Sum(v => v.rb_importe) + totalNc > decimal.Round(totalOperacion, 2))
            return "El documento no puede superar el saldo pendiente del pago.";

        foreach (var documento in documentos)
        {
            documento.ins_id = "DOC";
            documento.rb_fecha_valor = DateTime.SpecifyKind(documento.rb_fecha_valor.Date, DateTimeKind.Unspecified);
            documento.rb_rec = 0;
            documento.rb_opcion_cuota = "1";
            documento.rb_cupon_manual = "N";
            documento.rb_ch_dif = "N";
            documento.rb_estado = "N";
            documento.rb_aux = 0;
            documento.rb_dato1_valor = "";
            documento.rb_dato2_valor = "";
            documento.rb_dato3_valor = "";
            documento.id_externo = "";
        }
        return null;
    }
}
