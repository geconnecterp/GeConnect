using System.Globalization;
using System.Text.RegularExpressions;
using gc.infraestructura.Dtos.Cajas.Response;

namespace gc.caja.Models.Facturacion;

public sealed class FacturaEmitidaRequest
{
    public string TcoId { get; set; } = string.Empty;
    public string PuntoVenta { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;

    public bool TryNormalizar(out string tipo, out string comprobante, out string mensaje)
    {
        tipo = TcoId?.Trim() ?? string.Empty;
        comprobante = string.Empty;
        mensaje = string.Empty;
        var pv = PuntoVenta?.Trim() ?? string.Empty;
        var numero = Numero?.Trim() ?? string.Empty;
        if (!Regex.IsMatch(tipo, "^[0-9]{3}$"))
            mensaje = "Seleccione un tipo de comprobante valido.";
        else if (!Regex.IsMatch(pv, "^[0-9]{1,4}$"))
            mensaje = "El punto de venta debe tener entre 1 y 4 digitos.";
        else if (!Regex.IsMatch(numero, "^[0-9]{1,8}$"))
            mensaje = "El numero de comprobante debe tener entre 1 y 8 digitos.";
        else
            comprobante = $"{pv.PadLeft(4, '0')}-{numero.PadLeft(8, '0')}";
        return mensaje.Length == 0;
    }

    public static NCValidaResponseDto? SeleccionarUltimaRepeticion(
        IEnumerable<NCValidaResponseDto> candidatos, string tipo, string comprobante)
    {
        return candidatos
            .Where(x => x.tco_id?.Trim() == tipo && x.cm_compte?.Trim() == comprobante)
            .OrderByDescending(x => x.cm_repetido)
            .FirstOrDefault();
    }

    public static string ConstruirClave(string tipo, string comprobante, int? repetido)
    {
        // El SP lee la repeticion en la posicion 17 y admite un solo digito.
        if (!repetido.HasValue || repetido < 0 || repetido > 9)
            throw new ArgumentException("El comprobante tiene una repeticion incompatible con la carga de productos.");
        return tipo + comprobante + repetido.Value.ToString(CultureInfo.InvariantCulture);
    }
}
