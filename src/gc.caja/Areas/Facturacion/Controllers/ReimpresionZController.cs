using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class ReimpresionZController : ControladorBaseCaja
    {
        private readonly ICajaServicio _caja;
        public ReimpresionZController(IOptions<AppSettings> options, IHttpContextAccessor context,
            ILogger<ReimpresionZController> logger, ICajaServicio caja) : base(options, context, logger) { _caja = caja; }

        // Solo consulta configuración. No abrir caja ni invocar Valida_PV desde este módulo.
        private async Task<(CajaDatosDto? Caja, string Error)> ConsultarCaja()
        {
            try
            {
                var respuesta = await _caja.ObtenerDatosCF(CajaActual.CajaId, TokenCookie);
                var datos = respuesta.Entidad;
                if (!respuesta.Ok || datos == null) return (null, "No se pudo consultar el controlador de esta caja.");
                if (datos.caja_id?.Trim() != CajaActual.CajaId.Trim() || datos.adm_id?.Trim() != AdministracionId.Trim())
                    return (null, "La caja no corresponde a la sucursal de la sesión.");
                if (!ReimpresionZRangoDto.ControladorSoportado(datos.ctrl_id))
                    return (null, "Reimpresión Z requiere un controlador fiscal compatible. Disponible actualmente para Hasar 2G.");
                return (datos, string.Empty);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "No se pudo consultar disponibilidad de Reimpresión Z");
                return (null, "No se pudo verificar el controlador fiscal. Vuelva a ingresar al módulo cuando se restablezca la conexión.");
            }
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Disponibilidad()
        {
            if (!VerificarAutenticacion(out _)) return Unauthorized(new { habilitado = false });
            var contexto = await ConsultarCaja();
            return Json(new { habilitado = contexto.Caja != null, mensaje = contexto.Error });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!VerificarAutenticacion(out var redirect)) return redirect;
            var contexto = await ConsultarCaja();
            ViewBag.Habilitado = contexto.Caja != null;
            ViewBag.Error = contexto.Error;
            ViewBag.Caja = contexto.Caja?.caja_nombre ?? CajaActual.CajaId;
            ViewBag.Controlador = contexto.Caja?.ctrl_descripcion;
            ViewBag.Hoy = ReimpresionZRangoDto.Hoy.ToString("yyyy-MM-dd");
            ViewBag.MinHasta = ReimpresionZRangoDto.Hoy.AddYears(-5).ToString("yyyy-MM-dd");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Imprimir([FromBody] ReimpresionZRangoDto? rango)
        {
            if (!VerificarAutenticacion(out _)) return Unauthorized(new { ok = false, mensaje = "La sesión expiró. Ingrese nuevamente." });
            if (rango == null || !ModelState.IsValid) return BadRequest(new { ok = false, mensaje = "Los datos del rango no son válidos." });
            if (!rango.Validar(ReimpresionZRangoDto.Hoy, out _, out _, out var error))
                return Json(new { ok = false, mensaje = error });
            var contexto = await ConsultarCaja();
            if (contexto.Caja == null) return Json(new { ok = false, mensaje = contexto.Error });
            var resultado = await _caja.ReimprimirZ(new ReimpresionZRequestDto {
                caja_id = CajaActual.CajaId, usu_id = UserName, adm_id = AdministracionId,
                PorFecha = rango.PorFecha, Desde = rango.Desde, Hasta = rango.Hasta
            }, TokenCookie);
            var incierto = resultado.Entidad == null || resultado.Entidad.resultado == -9;
            return Json(new { ok = resultado.Ok, incierto,
                mensaje = resultado.Ok
                    ? "La solicitud finalizó sin errores informados. Verifique los reportes en el controlador fiscal."
                    : resultado.Mensaje });
        }
    }
}
