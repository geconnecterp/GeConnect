using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.caja.Controllers;

[Authorize]
public class EstadoOperacionController : ControladorBaseCaja
{
    public EstadoOperacionController(IOptions<AppSettings> opciones, IHttpContextAccessor contexto) : base(opciones, contexto) { }

    [HttpGet]
    public IActionResult Consultar() => Json(new { version = HttpContext.Session.GetString("CajaOperacionVersion") ?? "" });
}
