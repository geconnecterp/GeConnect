using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;

namespace gc.caja.Models.Estacion;

// El SP decide el documento. El dato local declara el papel instalado; no detecta hardware.
public sealed record ControlPapelImpresora(string Formato, string? Aviso)
{
    public static ControlPapelImpresora Evaluar(string? papelSp, string? papelFisico)
    {
        var formato = FormatoPapelComprobante.Resolver(papelSp);
        var fisico = papelFisico?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(fisico)) return new(formato, null);
        if (fisico is not ("A4" or "TK"))
            throw new ArgumentException("El papel declarado en el iniciador es inválido. Configure impresionFormatoFE como A4 o TK.");
        // EM es un canal de envío, no un tamaño de papel que deba coincidir con la impresora.
        if (formato == "EM") return new(formato, null);
        if (string.IsNullOrWhiteSpace(papelSp))
            return new(formato, $"GECO no informó el formato de papel del punto de venta. Se utilizará A4 por compatibilidad. El iniciador declara {Describir(fisico)}. Revise la configuración del punto de venta y del puesto antes de imprimir.");
        return new(formato, formato == fisico ? null
            : $"La configuración de impresión no coincide: GECO indica {Describir(formato)} y el iniciador declara {Describir(fisico)}. Se mantendrá el formato indicado por GECO. Revise la configuración del punto de venta y la impresora instalada antes de imprimir.");
    }

    public static string? AvisoPara(CajaSettings? caja, EstacionPuesto? puesto)
    {
        var datos = caja?.Caja;
        // Antes de recibir DATOS no existe un contrato del servidor para contrastar.
        if (datos == null || (string.IsNullOrWhiteSpace(datos.ctrl_id) && string.IsNullOrWhiteSpace(datos.ctrl_papel))) return null;
        if (PresentacionComprobante.Crear(datos.ctrl_id, caja!.Facturacion).Tipo != "FE") return null;
        try { return Evaluar(datos.ctrl_papel, puesto?.ImpresionFormatoFE).Aviso; }
        catch (ArgumentException ex) { return ex.Message; }
    }

    private static string Describir(string papel) => papel == "TK" ? "ticket de 80 mm (comandera)" : "hoja A4";
}
