using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RprResponseDto>))]
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

            var res = _almSv.ValidarUL(req.UL,req.AdmId);
            var response = new ApiResponse<RprResponseDto>(res);
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

            var res = _almSv.ValidarBox(req.Box,req.AdmId);
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
    }
}
