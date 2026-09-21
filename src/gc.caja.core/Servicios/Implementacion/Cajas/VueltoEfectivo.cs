using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Response;

namespace gc.caja.core.Servicios.Implementacion.Cajas;

public static class VueltoEfectivo
{
    public sealed record Resultado(bool Ok, string Mensaje, decimal Vuelto = 0m);

    // El navegador conserva lo recibido; al SP se informa sÃ³lo lo que queda en caja.
    // El catÃ¡logo EF proviene del servidor, nunca de una categorÃ­a enviada por el cliente.
    public static Resultado Normalizar(List<Json_Valor> valores, decimal total, decimal nc,
        IEnumerable<ValoresInsResDto> catalogoEfectivo)
    {
        if (total <= 0m || nc < 0m || nc > total ||
            decimal.Round(total, 2) != total || decimal.Round(nc, 2) != nc ||
            valores.Any(v => v.rb_importe <= 0m || decimal.Round(v.rb_importe, 2) != v.rb_importe))
            return new(false, "Los importes del pago deben ser vÃ¡lidos y tener hasta dos decimales. Las NC no pueden superar el total a pagar.");

        var excedente = valores.Sum(v => v.rb_importe) + nc - total;
        if (excedente < 0m)
            return new(false, "El total de los medios de pago y las Notas de CrÃ©dito no cubre el total a pagar.");
        if (excedente == 0m) return new(true, string.Empty);
        if (valores.Any(DocumentoCuentaCorriente.EsDocumento))
            return new(false, "Los documentos no permiten superar el total a pagar ni generar vuelto.");

        var idsEfectivo = catalogoEfectivo
            .Where(i => !string.IsNullOrWhiteSpace(i.ins_id) &&
                (string.IsNullOrWhiteSpace(i.tcf_id) || string.Equals(i.tcf_id.Trim(), "EF", StringComparison.OrdinalIgnoreCase)))
            .Select(i => i.ins_id.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var efectivo = valores.Where(v => idsEfectivo.Contains((v.ins_id ?? string.Empty).Trim())).ToList();
        if (efectivo.Any(v => v.rb_rec != 0m) || efectivo.Sum(v => v.rb_importe) < excedente)
            return new(false, "El vuelto sÃ³lo puede descontarse del efectivo ingresado. Ajuste los importes de los otros medios o de las NC.");

        // Descontar del Ãºltimo efectivo cargado, sin alterar otros valores ni las NC.
        var pendiente = excedente;
        foreach (var valor in efectivo.AsEnumerable().Reverse())
        {
            var descuento = Math.Min(pendiente, valor.rb_importe);
            valor.rb_importe -= descuento;
            pendiente -= descuento;
            if (pendiente == 0m) break;
        }
        valores.RemoveAll(v => v.rb_importe == 0m);
        for (var i = 0; i < valores.Count; i++) valores[i].rb_nro_valor = (i + 1).ToString("000");
        return new(true, string.Empty, excedente);
    }
}
