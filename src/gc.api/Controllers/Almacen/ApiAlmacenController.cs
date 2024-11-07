using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Deposito;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Windows.ApplicationModel.Resources.Core;

namespace gc.api.Controllers.Almacen
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiAlmacenController : ControllerBase
    {
        private readonly IApiAlmacenServicio _almSv;
        private readonly ILogger<ApiAlmacenController> _logger;

        public ApiAlmacenController(IApiAlmacenServicio apiAlmacenServicio, ILogger<ApiAlmacenController> logger)
        {
            _almSv = apiAlmacenServicio;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ValidarUL(RprABRequest req)
        {
            if (string.IsNullOrEmpty(req.UL) || string.IsNullOrWhiteSpace(req.UL))
            {
                return BadRequest("No se proporcionó la UL");
            }
            if (string.IsNullOrEmpty(req.AdmId) || string.IsNullOrWhiteSpace(req.AdmId))
            {
                return BadRequest("No se proporcionó la Administración actual.");
            }

            var res = _almSv.ValidarUL(req.UL, req.AdmId,req.Sm);
            var response = new ApiResponse<RespuestaDto>(res);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RprResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ValidarBox(RprABRequest req)
        {
            if (string.IsNullOrEmpty(req.Box) || string.IsNullOrWhiteSpace(req.Box))
            {
                return BadRequest("No se proporcionó el BOX");
            }
            if (string.IsNullOrEmpty(req.AdmId) || string.IsNullOrWhiteSpace(req.AdmId))
            {
                return BadRequest("No se proporcionó la Administración actual.");
            }

            var res = _almSv.ValidarBox(req.Box, req.AdmId);
            var response = new ApiResponse<RprResponseDto>(res);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RprResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult AlmacenaBoxUl(RprABRequest req)
        {
            if (string.IsNullOrEmpty(req.Box) || string.IsNullOrWhiteSpace(req.Box))
            {
                return BadRequest("No se proporcionó el BOX");
            }
            if (string.IsNullOrEmpty(req.UL) || string.IsNullOrWhiteSpace(req.UL))
            {
                return BadRequest("No se proporcionó la UL");
            }
            if (string.IsNullOrEmpty(req.AdmId) || string.IsNullOrWhiteSpace(req.AdmId))
            {
                return BadRequest("No se proporcionó la Administración actual.");
            }

            var res = _almSv.AlmacenaBoxUl(req);
            var response = new ApiResponse<RprResponseDto>(res);
            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AutorizacionTIDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult TRAutorizacionPendiente(string admId, string usuId, string titId)
        {
            if (string.IsNullOrEmpty(admId) || string.IsNullOrEmpty(usuId) || string.IsNullOrEmpty(titId))
            {
                return BadRequest("Faltó alguno de los datos necesarios para devolver las Autorizaciones Pendientes");
            }
            if (string.IsNullOrWhiteSpace(admId) || string.IsNullOrWhiteSpace(usuId) || string.IsNullOrWhiteSpace(titId))
            {
                return BadRequest("Faltó alguno de los datos necesarios para devolver las Autorizaciones Pendientes");
            }

            var lista = _almSv.TRObtenerAutorizacionesPendientes(admId, usuId, titId);

            var response = new ApiResponse<List<AutorizacionTIDto>>(lista);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetTIListaRubro(string tr, string admId, string usuId)
        {
            if (string.IsNullOrEmpty(tr) || string.IsNullOrEmpty(admId) || string.IsNullOrEmpty(usuId))
            {
                return BadRequest("Faltó alguno de los datos necesarios para devolver la lista de Rubros para TI");
            }
            if (string.IsNullOrWhiteSpace(tr) || string.IsNullOrWhiteSpace(admId) || string.IsNullOrWhiteSpace(usuId))
            {
                return BadRequest("Faltó alguno de los datos necesarios para devolver la lista de Rubros para TI");
            }
            var lista = _almSv.TIObtenerListaRubro(admId, usuId, tr);
            var response = new ApiResponse<List<BoxRubProductoDto>>(lista);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetTIListaBox(string tr, string admId, string usuId)
        {
            if (string.IsNullOrEmpty(tr) || string.IsNullOrEmpty(admId) || string.IsNullOrEmpty(usuId))
            {
                return BadRequest("Faltó alguno de los datos necesarios para devolver la lista de Box para TI");
            }
            if (string.IsNullOrWhiteSpace(tr) || string.IsNullOrWhiteSpace(admId) || string.IsNullOrWhiteSpace(usuId))
            {
                return BadRequest("Faltó alguno de los datos necesarios para devolver la lista de Box para TI");
            }
            var lista = _almSv.TIObtenerListaBox(admId, usuId, tr);
            var response = new ApiResponse<List<BoxRubProductoDto>>(lista);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult BuscaTIListaProductos(string tr, string admId, string usuId, string boxid, string rubroid)
        {
            if (string.IsNullOrEmpty(tr) || string.IsNullOrEmpty(admId) || string.IsNullOrEmpty(usuId) || string.IsNullOrEmpty(boxid) || string.IsNullOrEmpty(rubroid))
            {
                return BadRequest("Faltó alguno de los datos necesarios para devolver la lista de Box para TI");
            }
            if (string.IsNullOrWhiteSpace(tr) || string.IsNullOrWhiteSpace(admId) || string.IsNullOrWhiteSpace(usuId) || string.IsNullOrWhiteSpace(boxid) || string.IsNullOrWhiteSpace(rubroid))
            {
                return BadRequest("Faltó alguno de los datos necesarios para devolver la lista de Box para TI");
            }
            var lista = _almSv.BuscaTIListaProductos(admId, usuId, tr, boxid, rubroid);
            var response = new ApiResponse<List<TiListaProductoDto>>(lista);
            return Ok(response);
        }

		[HttpGet]
		[Route("[action]")]
		public IActionResult ObtenerListaDeBoxesPorDeposito(string depoId)
		{
			if (string.IsNullOrEmpty(depoId))
			{
				return BadRequest("Faltó especificar el ID de depósito.");
			}
			var lista = _almSv.ObtenerListaDeBoxesPorDeposito(depoId);
			var response = new ApiResponse<List<DepositoInfoBoxDto>>(lista);
			return Ok(response);
		}
	}
}
