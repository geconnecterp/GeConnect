using System.Collections.Concurrent;
using gc.caja.Controllers;
using gc.caja.Models.Administracion;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace gc.caja.Areas.Facturacion.Controllers;

[Area("Facturacion"), Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AdministradorCajaController : ControladorBaseCaja
{
    private readonly CajaGeneralServicio servicio;
    private readonly IMemoryCache cache;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Sucursales = new();
    public AdministradorCajaController(IOptions<AppSettings> options, IHttpContextAccessor context,
        ILogger<AdministradorCajaController> logger, CajaGeneralServicio servicio, IMemoryCache cache)
        : base(options, context, logger) { this.servicio = servicio; this.cache = cache; }

    // Administración requiere login y sucursal, no apertura ni validación operativa del PV. El puesto configurado se usa sólo para consultar datos.
    private bool SesionValida() => User.Identity?.IsAuthenticated == true && TieneTokenValido() &&
        !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(AdministracionId);
    [HttpGet]
    public IActionResult Index()
    {
        if (!SesionValida()) return RedirectToAction("Login", "Token", new { area = "Seguridad" });
        var nombre = User.Claims.FirstOrDefault(c => c.Type.Contains("AdmId"))?.Value.Split('#').ElementAtOrDefault(1);
        ViewBag.Sucursal = AdministracionId + (string.IsNullOrWhiteSpace(nombre) ? "" : " · " + nombre);
        return View();
    }
    [HttpGet]
    public async Task<IActionResult> Puestos()
    {
        if (!SesionValida()) return Unauthorized(new { ok = false, mensaje = "La sesión expiró. Ingrese nuevamente." });
        return Json(await servicio.ConsultarEstado(CajaActual?.CajaId ?? "", AdministracionId, TokenCookie));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Apertura(Guid solicitud) => Ejecutar(true, solicitud);
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Cierre(Guid solicitud) => Ejecutar(false, solicitud);
    private async Task<IActionResult> Ejecutar(bool apertura, Guid solicitud)
    {
        if (!SesionValida()) return Unauthorized(new ResultadoGeneral(false, "La sesión expiró. Ingrese nuevamente."));
        if (solicitud == Guid.Empty) return BadRequest(new ResultadoGeneral(false, "Identificador de solicitud inválido."));
        var sucursal = AdministracionId;
        var clave = $"caja-general:{sucursal}:{UserName}:{apertura}:{solicitud}";
        var puerta = Sucursales.GetOrAdd(sucursal, _ => new SemaphoreSlim(1, 1));
        if (!await puerta.WaitAsync(0)) return Conflict(new ResultadoGeneral(false, "Hay una operación general en curso para esta sucursal. Espere su resultado."));
        try
        {
            if (cache.TryGetValue(clave, out ResultadoGeneral? anterior)) return Json(anterior);
            var resultado = await servicio.Ejecutar(apertura, UserName, sucursal, CajaActual?.CajaId ?? "", TokenCookie);
            if (resultado.Ok || resultado.Incierto) cache.Set(clave, resultado, TimeSpan.FromHours(24));
            return Json(resultado);
        }
        finally { puerta.Release(); }
    }
}
