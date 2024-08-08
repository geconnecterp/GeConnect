using AutoMapper;
using gc.api.Controllers.Base;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using geco_0000.API.Controllers.Codigos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace gc.api.Controllers.Almacen
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class RubroController : ControladorBase
    {
        private readonly IMapper _mapper;
        private readonly IRubroServicio _rubSv;
        private readonly IUriService _uriService;
        private readonly ILogger<RubroController> _logger;

        public RubroController(IRubroServicio servicio, IMapper mapper, IUriService uriService, ILogger<RubroController> logger)
        {
            _rubSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }


        [HttpGet]
        [Route("[action]")]
        public IActionResult GetRubroLista()
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            List<RubroListaDto> rubros = _rubSv.GetRubroLista();
            var lista = _mapper.Map<List<RubroListaDto>>(rubros);

            var response = new ApiResponse<List<RubroListaDto>>(lista);
            return Ok(response);
        }
    }
}
