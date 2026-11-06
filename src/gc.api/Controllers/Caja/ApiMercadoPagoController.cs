using gc.api.core.Servicios.LineaCaja;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace gc.api.Controllers.Caja;

[Authorize,ApiController,Route("api/caja/mercadopago")]
public sealed class ApiMercadoPagoController : ControllerBase
{
    private readonly MercadoPagoServicio servicio;
    public ApiMercadoPagoController(MercadoPagoServicio servicio){this.servicio=servicio;}
    [HttpPost("opciones")]
    public IActionResult Opciones(MercadoPagoPeticionDto req)=>Ok(new ApiResponse<List<PagoIntegradoOpcionDto>>(servicio.Opciones(req.contexto,req.pago.ins_id)));
    [HttpPost("procesar")]
    public IActionResult Procesar(MercadoPagoPeticionDto req)=>Ok(new ApiResponse<RespuestaDto>(servicio.Procesar(req)));
}
