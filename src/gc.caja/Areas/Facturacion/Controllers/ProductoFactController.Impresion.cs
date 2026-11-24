using gc.caja.Models.Estacion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace gc.caja.Areas.Facturacion.Controllers;

public partial class ProductoFactController
{
    private string? PrepararFormatoComprobante(ReporteSolicitudDto solicitud, string letra)
    {
        // El puesto y DATOS ya fueron hidratados en el ingreso. No se cambia la operación registrada.
        var puesto = HttpContext.RequestServices.GetService<EstacionPuestoServicio>()?.Obtener(HttpContext);
        var formatoLocal = puesto?.ImpresionFormatoFE;
        var papelSp = CajaActual.Caja.ctrl_papel;
        string papel;
        try { papel = FormatoPapelComprobante.Resolver(string.IsNullOrWhiteSpace(formatoLocal) ? papelSp : formatoLocal); }
        catch (ArgumentException ex) { return ex.Message; }
        var fuente = !string.IsNullOrWhiteSpace(formatoLocal) ? "PUESTO" : !string.IsNullOrWhiteSpace(papelSp) ? "SP_CAJA_DATOS" : "COMPATIBILIDAD";
        _logger?.LogInformation("[REPORTES][FORMATO_FE] Caja={Caja} Sucursal={Sucursal} CtrlId={CtrlId} CtrlPapel={CtrlPapel} PreferenciaPuesto={Preferencia} Formato={Formato} Fuente={Fuente}",
            CajaActual.CajaId, AdministracionId, CajaActual.Caja.ctrl_id, papelSp, formatoLocal, papel, fuente);
        if (papel == "EM")
            return "El puesto solicita envío por email. La operación conserva su registro; el circuito de email está pendiente de configuración.";
        if (papel == "TK" && !string.Equals(letra.Trim(), "B", StringComparison.OrdinalIgnoreCase))
            return "El ticket de 80 mm está disponible para comprobantes B. El formato de Factura A está pendiente; utilice A4 para ese comprobante.";
        solicitud.Parametros["ctrl_papel"] = papel;
        return null;
    }
}
