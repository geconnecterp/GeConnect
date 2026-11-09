using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.caja.core.Servicios.Implementacion.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
namespace gc.caja.Areas.Facturacion.Controllers;

[Area("Facturacion"), Authorize, ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class MercadoPagoController : PagoConCreditosController
{
    private readonly MercadoPagoApiServicio mp;
    private readonly ICajaServicio caja;
    public MercadoPagoController(IOptions<AppSettings> options,ICheckoutServicio pago,IHttpContextAccessor context,ILogger<MercadoPagoController> logger,MercadoPagoApiServicio mp,ICajaServicio caja):base(options,pago,context,logger){this.mp=mp;this.caja=caja;}
    [HttpGet]
    public async Task<IActionResult> Opciones(string? insId,string coTipo)
    {
        try {
            if(!VerificarAutenticacion(out _)) return Unauthorized();
            var c=ContextoCobroIntegrado<PagoIntegradoContextoDto>(coTipo,"MP");
            var opciones=await mp.Opciones(new(){contexto=c,pago=new(){ins_id=insId ?? "",co_tipo=c.co_tipo}},TokenCookie);
            return Json(new{ok=true,opciones,mensaje=opciones.Count==0?"No hay cuotas Mercado Pago disponibles para esta operación.":"Opciones vigentes."});
        } catch(Exception ex){_logger?.LogWarning(ex,"[MP] Opciones no disponibles");return Json(new{ok=false,mensaje=ex.Message});}
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Procesar([FromBody] MercadoPagoPreparacionDto solicitud)
    {
        var enviado=false;
        try {
            if(!VerificarAutenticacion(out _)) return Unauthorized();
            if(solicitud==null) return BadRequest();
            solicitud.paymentid=(solicitud.paymentid ?? "").Trim().ToUpperInvariant();
            if(solicitud.paymentid.Length>0 && !Regex.IsMatch(solicitud.paymentid,"^[A-Z0-9]{4,30}$")) throw new InvalidOperationException("La carga manual requiere un ID de 4 a 30 caracteres alfanuméricos. Tres caracteres no se enviarán al Point.");
            if(solicitud.tipo is <1 or >3 || solicitud.cuotas_reales>1 && solicitud.tipo!=(short)TipoPagoPoint.Credito) throw new InvalidOperationException("Con más de una cuota sólo se admite Crédito; seleccione un tipo válido.");
            var c=ContextoCobroIntegrado<PagoIntegradoContextoDto>(solicitud.co_tipo,"MP");
            var catalogo=await PrepararCobroIntegrado(c,solicitud,caja,"Mercado Pago");
            enviado=true;
            var r=await mp.Procesar(new(){contexto=c,pago=solicitud},TokenCookie);
            if(r.resultado==0) RecordarCobroIntegrado(c.co_tipo,"T2",catalogo,r.resultado_id);
            _logger?.LogInformation("[MP] Operacion={Operacion}; Intento={Intento}; Resultado={Resultado}; Mensaje={Mensaje}",c.operacion_id,solicitud.intento_id,r.resultado,r.resultado_msj);
            return Json(r);
        } catch(Exception ex){_logger?.LogError(ex,"[MP] Procesamiento fallido. EnviadoAPI={Enviado}",enviado);return Json(new{resultado=-1,resultado_msj=ex.Message,sin_respuesta=enviado});}
    }
}
