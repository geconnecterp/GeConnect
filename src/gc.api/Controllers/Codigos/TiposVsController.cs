using AutoMapper;
using gc.infraestructura.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Codigos
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class TiposVsController : ControllerBase
    {
        private readonly IMapper _mapper;       
        private readonly IUriService _uriService;
        private readonly ILogger<TiposVsController> _logger;

        public TiposVsController( IMapper mapper, IUriService uriService, ILogger<TiposVsController> logger)
        {
           
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }



    }
}
