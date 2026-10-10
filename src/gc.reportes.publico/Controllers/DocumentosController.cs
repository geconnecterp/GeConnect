using Geco.Reportes.Publico.Models;
using Geco.Reportes.Publico.Services;
using Microsoft.AspNetCore.Mvc;

namespace Geco.Reportes.Publico.Controllers;

public sealed class DocumentosController(IReportePublicoService service) : Controller
{
    [HttpGet("/")]
    public IActionResult Inicio() => View("Aviso", new AvisoModel("Tus documentos, de forma directa",
        "Para descargar un documento, abrí el enlace que recibiste de Café América.", ""));

    [HttpGet("/d/{codigo}")]
    public async Task<IActionResult> Descargar(string codigo, CancellationToken cancellationToken)
    {
        // HEAD, prefetch y parámetros agregados no deben consumir descargas ni alterar el reporte.
        if (!HttpMethods.IsGet(Request.Method)) return StatusCode(405);
        if (Request.Query.Count != 0) return Aviso(400, "Enlace no válido", "Utilizá el enlace original, sin agregar parámetros.");
        if (Request.Headers["Sec-Purpose"].ToString().Contains("prefetch", StringComparison.OrdinalIgnoreCase)
            || Request.Headers["Purpose"].ToString().Contains("prefetch", StringComparison.OrdinalIgnoreCase)) return NoContent();
        try
        {
            var pdf = await service.DescargarAsync(codigo, new ContextoDescarga(
                HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(),
                HttpContext.TraceIdentifier), cancellationToken);
            return File(pdf.Contenido, "application/pdf", pdf.Nombre, enableRangeProcessing: false);
        }
        catch (DescargaException ex)
        {
            return Aviso(ex.Status, ex.Status == 410 ? "Enlace no disponible" : "Documento no disponible", ex.Message);
        }
    }

    private ViewResult Aviso(int status, string title, string message)
    {
        Response.StatusCode = status;
        return View("Aviso", new AvisoModel(title, message, HttpContext.TraceIdentifier));
    }
}
