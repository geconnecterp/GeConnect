using AutoMapper;
using gc.api.Controllers.Base;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Almacen.Tr.Request;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;

namespace gc.api.Controllers.Almacen
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiRemitoController : ApiControladorBase
    {
        private readonly IMapper _mapper;
        private readonly IRemitoServicio _remSv;
        private readonly IUriService _uriService;
        private readonly ILogger<ApiRemitoController> _logger;

        public ApiRemitoController(IRemitoServicio servicio, IMapper mapper, IUriService uriService, ILogger<ApiRemitoController> logger)
        {
            _remSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerRemitosTransferidosLista(string admId, string reeId = "%")
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            List<RemitoGenDto> remitos = _remSv.ObtenerRemitosPendientes(admId, reeId);
            var lista = _mapper.Map<List<RemitoGenDto>>(remitos);

            var response = new ApiResponse<List<RemitoGenDto>>(lista);
            return Ok(response);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult SetearEstado(RSetearEstadoRequest request)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            List<RespuestaDto> remitos = _remSv.SeteaEstado(request);
            var lista = _mapper.Map<List<RespuestaDto>>(remitos);

            var response = new ApiResponse<List<RespuestaDto>>(lista);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult VerConteos(string remCompte)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            List<RemitoVerConteoDto> remitos = _remSv.VerConteos(remCompte);
            var lista = _mapper.Map<List<RemitoVerConteoDto>>(remitos);

            var response = new ApiResponse<List<RemitoVerConteoDto>>(lista);
            return Ok(response);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult ConfirmarRecepcion(RConfirmaRecepcionRequest request)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            List<RespuestaDto> remitos = _remSv.ConfirmaRecepcion(request);
            var lista = _mapper.Map<List<RespuestaDto>>(remitos);

            var response = new ApiResponse<List<RespuestaDto>>(lista);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult VerificaProductoEnRemito(string remCompte, string pId)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            RespuestaDto resp = _remSv.VerificaProductoEnRemito(remCompte, pId);
            

            var response = new ApiResponse<RespuestaDto>(resp);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RespuestaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RTRCargarConteos(CargarJsonGenRequest request,bool esMod =false)
        {
            ApiResponse<RespuestaDto> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            RespuestaDto res = _remSv.RTRCargarConteos(request,esMod);

            response = new ApiResponse<RespuestaDto>(res);

            return Ok(response);
        }

		[HttpGet]
		[Route("[action]")]
		public IActionResult RTRCargarConteosXUL(string reCompte)
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			List<RTRxULDto> remitos = _remSv.RTRCargarConteosXUL(reCompte);
			var lista = _mapper.Map<List<RTRxULDto>>(remitos);

			var response = new ApiResponse<List<RTRxULDto>>(lista);
			return Ok(response);
		}
	}
}
