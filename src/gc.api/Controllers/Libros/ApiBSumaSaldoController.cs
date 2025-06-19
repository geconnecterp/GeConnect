using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.api.Controllers.Libros
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiBSumaSaldoController : ControllerBase
    {
        private readonly IApiSumaSaldoServicio _bssSv;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ApiBSumaSaldoController> _logger;

        public ApiBSumaSaldoController(
            IApiSumaSaldoServicio bssSv,
            IOptions<AppSettings> appSettings,
            ILogger<ApiBSumaSaldoController> logger)
        {
            _bssSv = bssSv;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        [HttpPost("obtener-balanceSS")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ApiResponse<List<BSumaSaldoRegDto>>> ObtenerBalanceSumaSaldos([FromBody] LibroFiltroDto request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<string>("El objeto de solicitud no puede ser nulo."));
            }

            var res = _bssSv.ObtenerBalanceSumaSaldos(request);
            if (res == null || res.Count == 0)
            {
                return NotFound(new ApiResponse<string>("No se encontraron registros para los criterios especificados."));
            }
            return Ok(new ApiResponse<List<BSumaSaldoRegDto>>(res));            
        }
    }
}
