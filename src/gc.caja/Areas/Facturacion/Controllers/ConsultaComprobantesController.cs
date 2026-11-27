using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using gc.caja.Controllers;
using gc.caja.Models.Facturacion;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Enumeraciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.caja.Areas.Facturacion.Controllers;

// Herramienta temporal de revisión: sólo consulta comprobantes y genera su representación PDF.
[Area("Facturacion"), Authorize, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ConsultaComprobantesController(IOptions<AppSettings> options, IHttpContextAccessor context,
    ILogger<ConsultaComprobantesController> logger, INotaCreditoServicio consulta,
    IReportesConfigService configuracion, IReportesService reportes) : ControladorBaseCaja(options, context, logger)
{
    [HttpGet]
    public IActionResult Index()
    {
        if (!VerificarAutenticacion(out var redirect)) return redirect;
        ViewBag.Sucursal = AdministracionName;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Tipos()
    {
        if (!VerificarAutenticacion(out _)) return SesionExpirada();
        try
        {
            var respuesta = await consulta.GetTipoComprobante("%", "VE", TokenCookie);
            if (respuesta?.Ok != true) return Error(respuesta?.Mensaje ?? "No se pudieron consultar los tipos de comprobante.");
            var tipos = (respuesta.ListaEntidad ?? []).Where(x => Regex.IsMatch(x.tco_id?.Trim() ?? "", "^[0-9]{3}$"))
                .Where(x => x.tco_letra?.Trim().ToUpperInvariant() is "A" or "B")
                .Select(x => new { id = x.tco_id.Trim(), descripcion = x.tco_desc?.Trim() ?? "", letra = x.tco_letra.Trim() }).ToList();
            return Json(new { ok = true, tipos });
        }
        catch (Exception ex) { logger.LogError(ex, "[REPORTES][CONSULTA_TEMPORAL] Error en catálogo."); return Error("No se pudieron consultar los tipos de comprobante."); }
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(2048)]
    public async Task<IActionResult> Buscar([FromBody] ConsultaComprobantesRequest? dato)
    {
        if (!VerificarAutenticacion(out _)) return SesionExpirada();
        if (dato == null || !ModelState.IsValid) return Error("Ingrese una identificación válida.");
        if (!dato.TryNormalizar(out var tipo, out var numero, out var error)) return Error(error);
        try
        {
            var (filas, mensaje) = await Consultar(tipo, numero);
            if (filas == null) return Error(mensaje);
            return Json(new { ok = true, comprobante = numero, candidatos = filas.Select(c => new {
                tipo = c.tco_id.Trim(), numero = c.cm_compte.Trim(), repeticion = c.cm_repetido!.Value,
                descripcion = c.tco_desc?.Trim(), letra = c.nc_tco_letra.Trim(), fecha = c.cm_fecha,
                cliente = c.cm_nombre?.Trim(), total = c.cm_total, moneda = c.mon_codigo?.Trim()
            }) });
        }
        catch (Exception ex) { logger.LogError(ex, "[REPORTES][CONSULTA_TEMPORAL] Error al buscar {Tipo}/{Numero}.", tipo, numero); return Error("No se pudo consultar el comprobante. Revise el log de Caja/API."); }
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(2048)]
    public async Task<IActionResult> Generar([FromBody] ConsultaComprobantesRequest? dato)
    {
        if (!VerificarAutenticacion(out _)) return SesionExpirada();
        if (dato == null || !ModelState.IsValid || dato.Repeticion is null) return Error("Seleccione el comprobante y su repetición.");
        if (!dato.TryNormalizar(out var tipo, out var numero, out var error)) return Error(error);
        var papel = dato.Formato?.Trim().ToUpperInvariant();
        if (papel is not ("A4" or "TK")) return Error("Seleccione A4 o ticket de 80 mm.");
        try
        {
            // Releer la identidad: no confiar en letra, sucursal o datos visibles enviados por el navegador.
            var (filas, mensaje) = await Consultar(tipo, numero);
            if (filas == null) return Error(mensaje);
            var seleccion = filas.Where(c => c.cm_repetido == dato.Repeticion).ToList();
            if (seleccion.Count != 1) return Error("La repetición seleccionada no identifica un único comprobante. Vuelva a buscarlo.");
            var letra = seleccion[0].nc_tco_letra.Trim().ToUpperInvariant();
            var config = configuracion.ObtenerPorKey(letra);
            if (config == null || !int.TryParse(config.Id, out var id) || !Enum.IsDefined(typeof(InfoReporte), id))
                return Error("No se encuentra configurado el reporte de letra " + letra + ".");
            var request = new ReporteSolicitudDto { Reporte = (InfoReporte)id, Formato = "P", Titulo = config.Nombre,
                Parametros = new() { ["tco_id"] = tipo, ["cm_compte"] = numero,
                    ["cm_repetido"] = dato.Repeticion.Value.ToString(CultureInfo.InvariantCulture), ["ctrl_papel"] = papel } };
            // Excepción de revisión explícita, aislada de PrepararFormatoComprobante y del puesto.
            Log("GENERAR REQUEST", new { solo_visualizacion = true, formato_manual = papel, request });
            var respuesta = await reportes.ObtenerPdfDesdeAPI(request, TokenCookie);
            Log("GENERAR RESPONSE", new { respuesta?.resultado, respuesta?.resultado_msj, pdf_caracteres = respuesta?.Base64?.Length ?? 0 });
            if (respuesta == null || respuesta.resultado != 0 || string.IsNullOrWhiteSpace(respuesta.Base64))
                return Error(respuesta?.resultado_msj ?? "Reportes no devolvió un PDF.");
            var pdf = Convert.FromBase64String(respuesta.Base64);
            if (pdf.Length < 5 || Encoding.ASCII.GetString(pdf, 0, 5) != "%PDF-") return Error("Reportes devolvió un archivo PDF inválido.");
            return File(pdf, "application/pdf");
        }
        catch (Exception ex) { logger.LogError(ex, "[REPORTES][CONSULTA_TEMPORAL] Error generando {Tipo}/{Numero}/{Formato}.", tipo, numero, papel); return Error("No se pudo generar la vista del comprobante. Revise el log de Reportes."); }
    }

    private async Task<(List<NCValidaResponseDto>? Filas, string Mensaje)> Consultar(string tipo, string numero)
    {
        var caja = CajaActual;
        if (caja?.Caja == null || string.IsNullOrWhiteSpace(AdministracionId) ||
            !string.Equals(caja.AdmId?.Trim(), AdministracionId.Trim(), StringComparison.Ordinal))
            return (null, "No se encontraron datos de caja para la sucursal de la sesión.");
        var proceso = Convert.ToString(caja.Caja.caja_nro_proceso, CultureInfo.InvariantCulture)?.Trim();
        if (string.IsNullOrEmpty(proceso) || !short.TryParse(Convert.ToString(caja.Caja.caja_nro_cierre, CultureInfo.InvariantCulture), out var cierre))
            return (null, "El puesto no informa proceso y cierre para consultar el comprobante.");
        var request = new NCValidaRequestDto { tco_id = tipo, cm_compte = numero, caja_nro_proceso = proceso, caja_nro_cierre = cierre };
        Log("BUSCAR REQUEST", request);
        // Del circuito NC sólo se reutiliza la consulta de cabecera. Los indicadores nc_* no restringen una visualización.
        var respuesta = await consulta.ValidarNC(request, TokenCookie);
        if (respuesta?.Ok != true) return (null, respuesta?.Mensaje ?? "No se pudo consultar el comprobante.");
        var filas = (respuesta.ListaEntidad ?? []).Where(c => c.tco_id?.Trim() == tipo && c.cm_compte?.Trim() == numero &&
            c.adm_id?.Trim() == AdministracionId.Trim() && c.cm_repetido is >= 0 and <= 9 && c.nc_tco_letra?.Trim().ToUpperInvariant() is "A" or "B").ToList();
        Log("BUSCAR RESPONSE", new { tipo, comprobante = numero, encontrados = filas.Count,
            candidatos = filas.Select(c => new { c.cm_repetido, c.nc_tco_letra, c.cm_fecha, c.cm_total, c.mon_codigo }) });
        return filas.Count > 0 ? (filas, "") : (null, "No se encontró un comprobante A o B con esos datos en la sucursal actual.");
    }
    private JsonResult Error(string mensaje) => Json(new { ok = false, mensaje });
    private IActionResult SesionExpirada() => Unauthorized(new { ok = false, mensaje = "La sesión expiró. Ingrese nuevamente." });
    private void Log(string evento, object datos) => logger.LogInformation("[REPORTES][CONSULTA_TEMPORAL][{Evento}] Solicitud={Solicitud}\n{Json}",
        evento, HttpContext.TraceIdentifier, CajaJsonDiagnostico.Formatear(JsonConvert.SerializeObject(datos)));
}