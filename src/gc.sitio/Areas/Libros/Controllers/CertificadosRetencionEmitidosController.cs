using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Implementacion.Libros;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Globalization;

namespace gc.sitio.Areas.Libros.Controllers;

[Area("Libros"), AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CertificadosRetencionEmitidosController(IOptions<AppSettings> options, IHttpContextAccessor contexto,
    ILogger<CertificadosRetencionEmitidosController> logger, CertificadosRetencionCliente cliente) : ControladorBase(options, contexto, logger)
{
    private readonly AppSettings settings = options.Value;
    private string Perfil() => JsonConvert.DeserializeObject<PerfilUserDto>(HttpContext.Session.GetString("UserPerfilSeleccionado") ?? "{}")?.perfil_id
        ?? throw new UnauthorizedAccessException("Debe seleccionar un perfil.");
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!VerificarAutenticacion(out var redirect)) return redirect;
        ViewData["Titulo"] = "Certificados de Retención Emitidos";
        var filtro = CertificadosRetencionFiltro.Inicial();
        ViewBag.Impuestos = new List<RetencionImpuesto>();
        try
        {
            var impuestos = await cliente.Opciones(TokenCookie, Perfil(), settings.MenuId, ct);
            ViewBag.Impuestos = impuestos;
            filtro.Impuesto = impuestos.FirstOrDefault(i => i.Soportado)?.Id ?? "";
            if (filtro.Impuesto == "") ViewBag.Error = "No hay impuestos implementados habilitados como agente de retención. Solicite revisar SPGECO_Tipos_Impuestos y agente_ret.";
        }
        catch (CertificadosRetencionHttpException ex) when (ex.Estado is 401 or 403) { return StatusCode(ex.Estado, ex.Message); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning("Retenciones: no se cargaron las opciones. Tipo {Tipo}", ex.GetType().Name);
            ViewBag.Error = "No se pudieron cargar los impuestos. Revise la conexión con la API y vuelva a ingresar.";
        }
        return View(filtro);
    }
    [HttpPost]
    public async Task<IActionResult> Ddjj(CertificadosRetencionDdjjPedido pedido, CancellationToken ct)
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
        catch (CertificadosRetencionHttpException ex) { return StatusCode(ex.Estado, new { mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning("Retenciones: DDJJ fallida. Tipo {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
            return StatusCode(ex is OperationCanceledException ? 504 : 503, new { mensaje = "No se pudo completar la descarga de la DDJJ mensual. No se entregó un archivo parcial. Revise la conexión con la API." });
        }
        finally { if (archivo != null) await archivo.DisposeAsync(); }
    }
    [HttpPost]
    public async Task<IActionResult> Consultar(CertificadosRetencionFiltro filtro, CancellationToken ct)
    {
        if (!VerificarAutenticacion(out _)) return Unauthorized(new { mensaje = "La sesión venció. Ingrese nuevamente." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Revise los valores de los filtros." });
        try
        {
            filtro.Validar();
            var consulta = await cliente.Consultar(filtro, TokenCookie, Perfil(), settings.MenuId, ct);
            return Json(CertificadosRetencionPresentacion.Crear(consulta, filtro));
        }
        catch (CertificadosRetencionHttpException ex) { return StatusCode(ex.Estado, new { mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning("Retenciones: consulta fallida. Tipo {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
            return StatusCode(ex is OperationCanceledException ? 504 : 503, new { mensaje = "No se pudo completar la consulta. No se muestran resultados parciales; revise la conexión con la API." });
        }
    }
}

// Importes formateados en C#, nunca totalizados o convertidos a Number en el navegador.
public static class CertificadosRetencionPresentacion
{
    public static object Crear(CertificadosRetencionConsulta consulta, CertificadosRetencionFiltro filtro)
    {
        var columnas = new List<object>();
        void Col(string titulo, bool numero = false) => columnas.Add(new { titulo, numero });
        Col("Fecha"); Col("Cuenta"); Col("CUIT"); Col("Nombre / Razón social"); Col("Certificado"); Col("Comprobante / OP");
        Col("Base imponible", true); Col("Alícuota %", true); Col("Retención", true); Col("Estado");
        if (filtro.Impuesto == "IB") { Col("Inscripción IB"); Col("Alícuota LH %", true); Col("Retención LH", true); }
        Col("Denominación de cuenta"); Col("Domicilio");
        string Texto(string? texto) => string.IsNullOrWhiteSpace(texto) ? "—" : texto.TrimEnd();
        string Numero(decimal? n) => n?.ToString("N2", CultureInfo.GetCultureInfo("es-AR")) ?? "—";
        var filas = consulta.Filas.Select(f =>
        {
            var celdas = new List<string> { f.fecha?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "—", Texto(f.cta_id), Texto(f.cuit), Texto(f.raz_soc),
                Texto(f.certificado), Texto(f.op_compte), Numero(f.@base), Numero(f.ali), Numero(f.reten), Texto(f.estado) };
            if (filtro.Impuesto == "IB") celdas.AddRange([Texto(f.ib_nro), Numero(f.cib_ali_lh), Numero(f.cib_reten_lh)]);
            celdas.AddRange([Texto(f.cta_denominacion), Texto(f.domicilio)]);
            return celdas;
        }).ToList();
        return new { columnas, filas, total = filas.Count, contexto = filtro.Impuesto + " · " + filtro.Descripcion,
            totalBase = CertificadosRetencionConsulta.Importe(consulta.TotalBase), totalRetencion = CertificadosRetencionConsulta.Importe(consulta.TotalRetencion),
            totalLH = filtro.Impuesto == "IB" ? CertificadosRetencionConsulta.Importe(consulta.TotalLH) : null,
            incompletos = consulta.ImportesIncompletos || (filtro.Impuesto == "IB" && !consulta.TotalLH.HasValue) };
    }
}
