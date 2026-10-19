using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Implementacion.Libros;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Libros.Controllers;

[Area("Libros")]
[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class IvaComprasDetalleController : ControladorBase
{
    private readonly IvaComprasCliente cliente;
    private readonly AppSettings settings;
    public IvaComprasDetalleController(IOptions<AppSettings> options, IHttpContextAccessor contexto,
        ILogger<IvaComprasDetalleController> logger, IvaComprasCliente cliente) : base(options, contexto, logger)
    { this.cliente = cliente; settings = options.Value; }

    private string Perfil() => JsonConvert.DeserializeObject<PerfilUserDto>(HttpContext.Session.GetString("UserPerfilSeleccionado") ?? "{}")?.perfil_id
        ?? throw new UnauthorizedAccessException("Debe seleccionar un perfil.");

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!VerificarAutenticacion(out var redirect)) return redirect;
        ViewData["Titulo"] = "Libro IVA Compras - Detalle";
        ViewBag.Opciones = new IvaComprasOpciones();
        try { ViewBag.Opciones = await cliente.Opciones(TokenCookie, Perfil(), settings.MenuId, ct); }
        catch (UnauthorizedAccessException) { return StatusCode(403, "El perfil no tiene acceso a Detalle IVA Compras (imp_lc_det)."); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            ViewBag.Error = "No se pudieron cargar los filtros. Verifique la conexión y los SP de listas; luego vuelva a ingresar.";
            _logger?.LogWarning("No se cargaron filtros IVA: {Tipo}", ex.GetType().Name);
        }
        // Consulta ancha: comenzar con 100 filas sin modificar la configuración global.
        return View(IvaComprasFiltro.Inicial());
    }

    [HttpPost]
    public async Task<IActionResult> Consultar(IvaComprasFiltro filtro, CancellationToken ct)
    {
        if (!VerificarAutenticacion(out _)) return Unauthorized(new { mensaje = "La sesión venció. Ingrese nuevamente." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Revise los valores de los filtros." });
        try
        {
            filtro.Validar();
            var pagina = await cliente.Consultar(filtro, TokenCookie, Perfil(), settings.MenuId, ct);
            ViewBag.Filtros = filtro.Descripcion;
            return PartialView("_Grilla", pagina);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (IvaComprasDatoInvalidoException ex)
        {
            _logger?.LogWarning("Dato IVA inválido en consulta. Campo: {Campo}", ex.Campo);
            return UnprocessableEntity(new { mensaje = ex.Message });
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, new { mensaje = "No se pudo comunicar con la API de consulta. Revise la conexión y vuelva a intentar." });
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return StatusCode(504, new { mensaje = "La consulta excedió el tiempo de espera. Intente acotar los filtros y vuelva a consultar." });
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            _logger?.LogWarning("Consulta IVA fallida: {Tipo}", ex.GetType().Name);
            return StatusCode(503, new { mensaje = "Se produjo un error al procesar el Libro IVA. No se muestran resultados parciales. Contacte al administrador para revisar el diagnóstico de la API." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ExportarExcel(IvaComprasFiltro filtro, CancellationToken ct)
    {
        if (!VerificarAutenticacion(out _)) return Unauthorized(new { mensaje = "La sesión venció. Ingrese nuevamente." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Revise los valores de los filtros." });
        try
        {
            filtro.Validar(exportacion: true);
            var archivo = await cliente.Excel(filtro, TokenCookie, Perfil(), settings.MenuId, ct);
            return File(archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Libro_IVA_Compras_{filtro.PeriodoTexto}.xlsx");
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (IvaComprasDatoInvalidoException ex)
        {
            _logger?.LogWarning("Dato IVA inválido en exportación. Campo: {Campo}", ex.Campo);
            return UnprocessableEntity(new { mensaje = ex.Message });
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            _logger?.LogWarning("Exportación IVA fallida: {Tipo}", ex.GetType().Name);
            return StatusCode(503, new { mensaje = ex is InvalidOperationException ? ex.Message : "No se pudo completar la exportación. No se entregó un archivo parcial." });
        }
    }
}
