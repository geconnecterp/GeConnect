using gc.caja.core.Servicios.Contratos.Cajas;
using gc.caja.core.Servicios.Implementacion.Cajas;
using gc.infraestructura.Core.ReglasCaja;
using gc.infraestructura.Dtos.Cajas;
using Newtonsoft.Json;
namespace gc.caja.Controllers;

public abstract partial class PagoConCreditosController
{
    protected async Task<Dictionary<string,string>> PrepararCobroIntegrado(PagoIntegradoContextoDto c, PagoIntegradoPreparacionDto solicitud, ICajaServicio cajaServicio, string nombre)
    {
        if (c.co_tipo is "CR" or "CF" && (string.IsNullOrWhiteSpace(LP_Id) || FacturaProductos.Count == 0 || FacturaSubtotales.Count == 0))
            throw new InvalidOperationException("Debe cargar productos, calcular totales y tener una lista de precios activa antes de cobrar con " + nombre + ".");
        var catalogo = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        var medios = await _pagoFactServicio.ObtenerValoresMP(new(){co_tipo=c.co_tipo,cta_id=c.cta_id,adm_id=c.adm_id},TokenCookie);
        if (!medios.Ok) throw new InvalidOperationException(medios.Mensaje);
        foreach(var medio in medios.ListaEntidad ?? []) {
            var instrumentos=await _pagoFactServicio.ObtenerValoresIns(new(){tcf_id=medio.tcf_id ?? "",co_tipo=c.co_tipo,cta_id=c.cta_id,adm_id=c.adm_id},TokenCookie);
            if(!instrumentos.Ok) throw new InvalidOperationException(instrumentos.Mensaje);
            foreach(var i in instrumentos.ListaEntidad ?? []) if(!string.IsNullOrWhiteSpace(i.ins_id)) catalogo[i.ins_id.Trim()]=(medio.tcf_id ?? "").Trim().ToUpperInvariant();
        }
        foreach(var par in LeerCatalogoIntegrado(c.co_tipo)) catalogo.TryAdd(par.Key,par.Value);
        var valores=solicitud.valores ?? [];
        if(c.co_tipo is not ("CV" or "IV")) {
            var nc=await ValidarYConstruirUnionesNcAsync(solicitud.uniones,c.co_tipo,c.co_tipo=="CF");
            if(!nc.Ok) throw new InvalidOperationException(nc.Mensaje);
            var total=TotalBasePagoIntegrado(c.co_tipo)+PagosIntegrados.TotalAjustes(valores,catalogo);
            if(total<=0 || solicitud.monto_base<=0 || solicitud.monto_base>total-valores.Sum(v=>v.rb_importe)-nc.TotalImputado)
                throw new InvalidOperationException("El monto de " + nombre + " supera el saldo real pendiente de la operación.");
            var regla=ReglasCreditoPago.Validar(valores,total,nc.TotalImputado,c.co_tipo,ReglasCreditoPago.ObtenerCondiciones(ClienteActual!),catalogo,DateTime.Today);
            if(!regla.Ok) throw new InvalidOperationException(regla.Mensaje);
        }
        if(c.co_tipo=="CD" && solicitud.recargo!=0) throw new InvalidOperationException("Cobranza Diferida no admite recargos financieros.");
        var pv=await ValidarEstadoPuntoVenta(cajaServicio,c.caja_id,CajaActual.Caja.ctrl_id ?? "",c.caja_nro_proceso,c.caja_nro_cierre.ToString(),"F");
        if(!pv.PuedeContinuar) throw new InvalidOperationException(pv.Mensaje);
        GuardarCatalogoIntegrado(c.co_tipo,catalogo);return catalogo;
    }
    protected void RecordarCobroIntegrado(string coTipo, string medio, Dictionary<string,string> catalogo, string valoresJson)
    {
        foreach(var v in JsonConvert.DeserializeObject<List<Json_Valor>>(valoresJson) ?? []) if(!string.IsNullOrWhiteSpace(v.ins_id)) catalogo[v.ins_id]=medio;
        GuardarCatalogoIntegrado(coTipo,catalogo);
    }
}
