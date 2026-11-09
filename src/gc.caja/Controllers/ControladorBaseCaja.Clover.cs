using gc.infraestructura.Dtos.Cajas;
using gc.caja.core.Servicios.Implementacion.Cajas;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace gc.caja.Controllers;

public partial class ControladorBaseCaja
{
    protected CloverContextoDto ContextoClover(string coTipo) => ContextoCobroIntegrado<CloverContextoDto>(coTipo, "CLOVER");
    protected T ContextoCobroIntegrado<T>(string coTipo, string proveedor) where T : PagoIntegradoContextoDto, new()
    {
        var caja = CajaActual; var cliente = ClienteActual;
        if (caja?.Caja == null || cliente == null || !short.TryParse(caja.Caja.caja_nro_cierre, out var cierre) || cierre < 0 || string.IsNullOrWhiteSpace(caja.Caja.caja_nro_proceso)) throw new InvalidOperationException("Debe seleccionar cliente y tener abierto el puesto de Caja.");
        coTipo = coTipo.Trim().ToUpperInvariant();
        var registrado = string.Equals(cliente.Origen?.Trim(), "C", StringComparison.OrdinalIgnoreCase);
        if (coTipo is "CR" or "CF" && coTipo != (registrado ? "CR" : "CF")) throw new InvalidOperationException("El tipo de operación no corresponde al cliente seleccionado.");
        if (coTipo is "CV" or "IV" && (!registrado || string.IsNullOrWhiteSpace(cliente.cta_id))) throw new InvalidOperationException("Cambio e ingreso de valores requieren cliente registrado.");
        object detalle = coTipo switch {
            "CR" or "CF" => new { productos = FacturaProductos, subtotales = FacturaSubtotales },
            "CD" => FacturasSeleccionadasParaCobro,
            "CC" => CuentaCorrienteDelClienteSeleccionadaParaElCobro,
            "CV" or "IV" => new { tipo = coTipo },
            _ => throw new InvalidOperationException("El módulo no admite este pago integrado.") };
        var identidad = registrado ? cliente.cta_id : cliente.cta_documento;
        var huella = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { caja.CajaId, caja.AdmId, caja.Caja.caja_nro_proceso, cierre, identidad, coTipo, detalle }))));
        var key = proveedor + "_OP:" + huella;
        var id = HttpContext.Session.GetString(key);
        if (id == null) { id = Guid.NewGuid().ToString(); HttpContext.Session.SetString(key, id); }
        return new() { adm_id = caja.AdmId ?? AdministracionId, caja_id = caja.CajaId, caja_nro_proceso = caja.Caja.caja_nro_proceso, caja_nro_cierre = cierre, cta_id = registrado ? cliente.cta_id ?? "" : "", usu_id = UserName ?? "", co_tipo = coTipo, operacion_id = id, huella = huella };
    }
    protected decimal TotalBaseClover(string coTipo) => TotalBasePagoIntegrado(coTipo);
    protected decimal TotalBasePagoIntegrado(string coTipo) => coTipo switch {
        "CR" or "CF" => FacturaSubtotales?.Sum(s => s.importe) ?? 0,
        "CD" => FacturasSeleccionadasParaCobro?.Sum(f => f.cv_importe) ?? 0,
        "CC" => CuentaCorrienteDelClienteSeleccionadaParaElCobro?.Sum(f => f.cv_importe) ?? 0,
        _ => 0 };
    protected void LimpiarOperacionClover(CloverContextoDto c) => LimpiarOperacionIntegrada(c, "CLOVER");
    protected void LimpiarOperacionIntegrada(PagoIntegradoContextoDto c, string proveedor) => HttpContext.Session.Remove(proveedor + "_OP:" + c.huella);
    protected string ClaveCatalogoIntegrado(string coTipo) => $"PAGO_CATALOGO:{CajaActual.CajaId}:{coTipo}:{ClienteActual!.Origen}:{ClienteActual!.cta_id}:{ClienteActual!.cta_documento}";
    protected Dictionary<string,string> LeerCatalogoIntegrado(string coTipo) => new(JsonConvert.DeserializeObject<Dictionary<string,string>>(HttpContext.Session.GetString(ClaveCatalogoIntegrado(coTipo)) ?? "{}") ?? [], StringComparer.OrdinalIgnoreCase);
    protected void GuardarCatalogoIntegrado(string coTipo, Dictionary<string,string> catalogo) => HttpContext.Session.SetString(ClaveCatalogoIntegrado(coTipo), JsonConvert.SerializeObject(catalogo));
}
