using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.DocManager;
using gc.sitio.core.Servicios.Implementacion.Libros;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Libros.Controllers;

[Area("Libros"), AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class IvaComprasResumenController(IOptions<AppSettings> options, IHttpContextAccessor contexto,
    ILogger<IvaComprasResumenController> logger, IvaComprasResumenCliente cliente, IOptions<DocsManager> docs,
    IDocManagerServicio gestor) : ControladorBase(options, contexto, logger)
{
    private string Perfil() => JsonConvert.DeserializeObject<PerfilUserDto>(HttpContext.Session.GetString("UserPerfilSeleccionado") ?? "{}")?.perfil_id
        ?? throw new UnauthorizedAccessException("Debe seleccionar un perfil.");

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!VerificarAutenticacion(out var redirect)) return redirect;
        ViewData["Titulo"] = "Libro IVA Compras - Consulta Resumen";
        try
        {
            var perfil = Perfil();
            await cliente.Acceso(TokenCookie, perfil, options.Value.MenuId, ct);
            var modulo = docs.Value.Modulos.Single(m => m.Id == IvaComprasResumenDocumentos.Modulo);
            DocumentManager = gestor.InicializaObjeto(modulo.Titulo, modulo);
            ArchivosCargadosModulo = gestor.GeneraArbolArchivos(modulo);
            ViewBag.Perfil = perfil;
            ViewBag.Menu = options.Value.MenuId;
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, ex.Message); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning("No se pudo abrir Resumen IVA: {Tipo}", ex.GetType().Name);
            ViewBag.Error = "No se pudo verificar el acceso al módulo. Revise la conexión con la API y vuelva a ingresar.";
        }
        return View(IvaComprasResumenFiltro.Inicial());
    }

    [HttpPost]
    public Task<IActionResult> Consultar(IvaComprasResumenFiltro filtro, CancellationToken ct) => Ejecutar(filtro, false, ct);
    [HttpPost]
    public Task<IActionResult> ExportarExcel(IvaComprasResumenFiltro filtro, CancellationToken ct) => Ejecutar(filtro, true, ct);

    private async Task<IActionResult> Ejecutar(IvaComprasResumenFiltro filtro, bool excel, CancellationToken ct)
    {
        if (!VerificarAutenticacion(out _)) return Unauthorized(new { mensaje = "La sesión venció. Ingrese nuevamente." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Revise el año y el mes seleccionados." });
        try
        {
            filtro.Validar();
            if (excel) return File(await cliente.Excel(filtro, TokenCookie, Perfil(), options.Value.MenuId, ct),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Libro_IVA_Compras_Resumen_{filtro.PeriodoTexto}.xlsx");
            return PartialView("_Grilla", await cliente.Consultar(filtro, TokenCookie, Perfil(), options.Value.MenuId, ct));
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        { return StatusCode(504, new { mensaje = "La consulta excedió el tiempo de espera. Intente nuevamente; no se muestran datos parciales." }); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning("Operación IVA resumen fallida. Tipo {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
            return StatusCode(503, new { mensaje = "No se pudo completar la operación. Verifique la conexión con las API y el SP. No se entregaron datos parciales." });
        }
    }
}
