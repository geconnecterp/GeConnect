using AutoMapper;
using gc.api.Controllers.Base;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult ObtenerRemitosTransferidosLista(string admId)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            List<RemitoTransferidoDto> remitos = _remSv.ObtenerRemitosTransferidos(admId);
            var lista = _mapper.Map<List<RemitoTransferidoDto>>(remitos);

            var response = new ApiResponse<List<RemitoTransferidoDto>>(lista);
            return Ok(response);
        }
    }
}
