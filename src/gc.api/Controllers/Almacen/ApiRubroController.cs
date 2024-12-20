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
    public class ApiRubroController : ApiControladorBase
    {
        private readonly IMapper _mapper;
        private readonly IRubroServicio _rubSv;
        private readonly IUriService _uriService;
        private readonly ILogger<ApiRubroController> _logger;


        /// <summary>
        /// Utilizaré en conjunto este controlador para operar Rubro y BOX
        /// </summary>
        /// <param name="servicio"></param>
        /// <param name="mapper"></param>
        /// <param name="uriService"></param>
        /// <param name="logger"></param>
        public ApiRubroController(IRubroServicio servicio, IMapper mapper, IUriService uriService, ILogger<ApiRubroController> logger)
        {
            _rubSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }


        [HttpGet]
        [Route("[action]")]
        public IActionResult GetRubroLista(string? cta_id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            if (string.IsNullOrEmpty(cta_id)) { cta_id = "%"; }
            
            List<RubroListaDto> rubros = _rubSv.GetRubroLista(cta_id);

            var lista = _mapper.Map<List<RubroListaDto>>(rubros);

            var response = new ApiResponse<List<RubroListaDto>>(lista);
            return Ok(response);
        }

       
    }
}

