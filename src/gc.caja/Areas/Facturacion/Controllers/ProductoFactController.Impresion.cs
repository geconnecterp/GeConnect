using gc.caja.Models.Estacion;
using gc.infraestructura.Dtos.Gen;
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
        ControlPapelImpresora control;
        try { control = ControlPapelImpresora.Evaluar(papelSp, formatoLocal); }
        catch (ArgumentException ex) { return ex.Message; }
        var papel = control.Formato;
        var fuente = !string.IsNullOrWhiteSpace(papelSp) ? "SP_CAJA_DATOS" : "COMPATIBILIDAD";
        _logger?.LogInformation("[REPORTES][FORMATO_FE] Caja={Caja} Sucursal={Sucursal} CtrlId={CtrlId} CtrlPapel={CtrlPapel} PapelFisicoPuesto={PapelFisico} Formato={Formato} Fuente={Fuente}",
            CajaActual.CajaId, AdministracionId, CajaActual.Caja.ctrl_id, papelSp, formatoLocal, papel, fuente);
        if (control.Aviso != null)
            _logger?.LogWarning("[REPORTES][PAPEL_DIVERGENTE] Caja={Caja} Sucursal={Sucursal} CtrlPapel={CtrlPapel} PapelFisicoPuesto={PapelFisico} Aviso={Aviso}",
                CajaActual.CajaId, AdministracionId, papelSp, formatoLocal, control.Aviso);
        if (papel == "EM")
            return "GECO indica envío por email. La operación conserva su registro; el circuito de email está pendiente de configuración.";
        if (papel == "TK" && letra.Trim().ToUpperInvariant() is not ("A" or "B"))
            return "El ticket de 80 mm está disponible para comprobantes A y B. Revise el tipo de comprobante solicitado.";
        solicitud.Parametros["ctrl_papel"] = papel;
        return null;
    }
}
