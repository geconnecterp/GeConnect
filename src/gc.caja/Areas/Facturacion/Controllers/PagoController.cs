using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class PagoController : ControladorBaseCaja
    {
        private readonly IPagoFactServicio _pagoFactServicio;

        public PagoController(IOptions<AppSettings> options,
            IPagoFactServicio pagoFactServicio,
            IHttpContextAccessor httpContext,
            ILogger<PagoController> logger) : base(options, httpContext, logger)
        {
            _pagoFactServicio = pagoFactServicio;
        }

        /// <summary>
        /// Vista principal del módulo de pagos
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        //crear codigo simple post para llamar los metodos del servicio IPagoFactServicio y retornar la respuesta en formato JSON
        [HttpPost]
        public async Task<IActionResult> ObtenerValoresPendientes([FromBody] ValoresPendientesReqDto req)
        {

            var res = await _pagoFactServicio.ObtenerValoresPendientes(req, TokenCookie);
            return Json(res);
        }
        [HttpPost]
        public async Task<IActionResult> ObtenerValoresNC([FromBody] ValoresNCReqDto req)
        {

            var res = await _pagoFactServicio.ObtenerValoresNC(req, TokenCookie);
            return Json(res);
        }
        [HttpPost]
        public async Task<IActionResult> ObtenerValoresMP([FromBody] ValoresMPReqDto req)
        {

            var res = await _pagoFactServicio.ObtenerValoresMP(req, TokenCookie);
            return Json(res);
        }
        [HttpPost]
        public async Task<IActionResult> ObtenerValoresIns([FromBody] ValoresInsReqDto req)
        {
          
            var res = await _pagoFactServicio.ObtenerValoresIns(req, TokenCookie);
            return Json(res);
        }
    }
}
