using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Implementacion.Libros;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Libros.Controllers;

[Area("Libros"), AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CertificadosSufridosController(IOptions<AppSettings> options, IHttpContextAccessor contexto,
    ILogger<CertificadosSufridosController> logger, CertificadosSufridosCliente cliente) : ControladorBase(options, contexto, logger)
{
    private readonly AppSettings settings = options.Value;
    private string Perfil() => JsonConvert.DeserializeObject<PerfilUserDto>(HttpContext.Session.GetString("UserPerfilSeleccionado") ?? "{}")?.perfil_id
        ?? throw new UnauthorizedAccessException("Debe seleccionar un perfil.");
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!VerificarAutenticacion(out var redirect)) return redirect;
        ViewData["Titulo"] = "Certificados de Retención y Percepciones Sufridos";
        var filtro = CertificadosSufridosFiltro.Inicial(); ViewBag.Impuestos = new List<SufridoImpuesto>();
        try
        {
            var impuestos = await cliente.Opciones(TokenCookie, Perfil(), settings.MenuId, ct);
            ViewBag.Impuestos = impuestos; filtro.Impuesto = impuestos.FirstOrDefault()?.Id ?? "";
            if (filtro.Impuesto == "") ViewBag.Error = "No hay impuestos habilitados como contribuyente. Solicite revisar SPGECO_Tipos_Impuestos y cont = S.";
        }
        catch (CertificadosSufridosHttpException ex) when (ex.Estado is 401 or 403) { return StatusCode(ex.Estado, ex.Message); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning("Certificados sufridos: opciones no disponibles. Tipo {Tipo}", ex.GetType().Name);
            ViewBag.Error = "No se pudieron cargar los impuestos. Revise la conexión con la API y vuelva a ingresar.";
        }
        return View(filtro);
    }
    [HttpPost]
    public async Task<IActionResult> Consultar(CertificadosSufridosFiltro filtro, CancellationToken ct)
    {
        if (!VerificarAutenticacion(out _)) return Unauthorized(new { mensaje = "La sesión venció. Ingrese nuevamente." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Revise los valores de los filtros." });
        try
        {
            filtro.Validar();
            return Json(CertificadosSufridosPresentacion.Crear(await cliente.Consultar(filtro, TokenCookie, Perfil(), settings.MenuId, ct), filtro));
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex); }
    }
    [HttpPost]
    public async Task<IActionResult> Pdf(CertificadosSufridosFiltro filtro, CancellationToken ct)
    {
        if (!VerificarAutenticacion(out _)) return Unauthorized(new { mensaje = "La sesión venció. Ingrese nuevamente." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Revise los valores de los filtros del reporte." });
        try
        {
            filtro.Validar();
            var bytes = await cliente.Pdf(filtro, TokenCookie, Perfil(), settings.MenuId, ct);
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(bytes, "application/pdf", filtro.NombrePdf);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex); }
    }
    private IActionResult Error(Exception ex)
    {
        if (ex is CertificadosSufridosHttpException http) return StatusCode(http.Estado, new { mensaje = http.Message });
        if (ex is ArgumentException) return BadRequest(new { mensaje = ex.Message });
        if (ex is NotSupportedException) return UnprocessableEntity(new { mensaje = ex.Message });
        logger.LogWarning("Certificados sufridos: operación fallida. Tipo {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
        return StatusCode(ex is OperationCanceledException ? 504 : 503, new { mensaje = "No se pudo completar la consulta o reporte. Revise la conexión y el contrato del SP. No se entregaron resultados parciales." });
    }
}
