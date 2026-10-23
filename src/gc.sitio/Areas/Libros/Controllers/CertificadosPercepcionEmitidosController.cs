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
public class CertificadosPercepcionEmitidosController(IOptions<AppSettings> options, IHttpContextAccessor contexto,
    ILogger<CertificadosPercepcionEmitidosController> logger, CertificadosPercepcionCliente cliente) : ControladorBase(options, contexto, logger)
{
    private readonly AppSettings settings = options.Value;
    private string Perfil() => JsonConvert.DeserializeObject<PerfilUserDto>(HttpContext.Session.GetString("UserPerfilSeleccionado") ?? "{}")?.perfil_id
        ?? throw new UnauthorizedAccessException("Debe seleccionar un perfil.");
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!VerificarAutenticacion(out var redirect)) return redirect;
        ViewData["Titulo"] = "Certificados de Percepción Emitidos";
        var filtro = CertificadosPercepcionFiltro.Inicial(); ViewBag.Impuestos = new List<PercepcionImpuesto>();
        try
        {
            var impuestos = await cliente.Opciones(TokenCookie, Perfil(), settings.MenuId, ct);
            ViewBag.Impuestos = impuestos; filtro.Impuesto = impuestos.FirstOrDefault()?.Id ?? "";
            if (filtro.Impuesto == "") ViewBag.Error = "No hay impuestos IB/IV habilitados como agente de percepción. Solicite revisar SPGECO_Tipos_Impuestos y agente_per.";
        }
        catch (CertificadosPercepcionHttpException ex) when (ex.Estado is 401 or 403) { return StatusCode(ex.Estado, ex.Message); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning("Percepciones: no se cargaron las opciones. Tipo {Tipo}", ex.GetType().Name);
            ViewBag.Error = "No se pudieron cargar los impuestos. Revise la conexión con la API y vuelva a ingresar.";
        }
        return View(filtro);
    }
    [HttpPost]
    public async Task<IActionResult> Ddjj(CertificadosPercepcionDdjjPedido pedido, CancellationToken ct)
    {
        if (!VerificarAutenticacion(out _)) return Unauthorized(new { mensaje = "La sesión venció. Ingrese nuevamente." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Revise el impuesto y período de la DDJJ." });
        FileStream? archivo = null;
        try
        {
            pedido.Validar();
            archivo = await cliente.Ddjj(pedido, TokenCookie, Perfil(), settings.MenuId, ct);
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            var resultado = File(archivo, pedido.ContentType, pedido.Nombre); archivo = null;
            return resultado;
        }
        catch (CertificadosPercepcionHttpException ex) { return StatusCode(ex.Estado, new { mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning("Percepciones: DDJJ fallida. Tipo {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
            return StatusCode(ex is OperationCanceledException ? 504 : 503, new { mensaje = "No se pudo completar la descarga de la DDJJ mensual. No se entregó un archivo parcial. Revise la conexión con la API." });
        }
        finally { if (archivo != null) await archivo.DisposeAsync(); }
    }
    [HttpPost]
    public async Task<IActionResult> Consultar(CertificadosPercepcionFiltro filtro, CancellationToken ct)
    {
        if (!VerificarAutenticacion(out _)) return Unauthorized(new { mensaje = "La sesión venció. Ingrese nuevamente." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Revise los valores de los filtros." });
        try
        {
            filtro.Validar();
            var consulta = await cliente.Consultar(filtro, TokenCookie, Perfil(), settings.MenuId, ct);
            return Json(CertificadosPercepcionPresentacion.Crear(consulta, filtro));
        }
        catch (CertificadosPercepcionHttpException ex) { return StatusCode(ex.Estado, new { mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning("Percepciones: consulta fallida. Tipo {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
            return StatusCode(ex is OperationCanceledException ? 504 : 503, new { mensaje = "No se pudo completar la consulta. No se muestran resultados parciales; revise la conexión con la API." });
        }
    }
}
