using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Net;
using System.Reflection;
using System.Text.Json;

namespace gc.api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IMapper _mapper;
        private ITestServicio _testSv;
        private readonly IUriService _uriService;
        private readonly ILogger<TestController> _logger;

        public TestController(ITestServicio servicio, IMapper mapper, IUriService uriService, ILogger<TestController> logger)
        {
            _testSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }


        [HttpGet(Name = nameof(GetTests))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<TestDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult GetTests([FromQuery] QueryFilters filters)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var tests = _testSv.GetAll(filters);
            var testDtos = _mapper.Map<IEnumerable<TestDto>>(tests);

            // presentando en el header información basica sobre la paginación
            var metadata = new Metadata
            {
                TotalCount = tests.TotalCount,
                PageSize = tests.PageSize,
                CurrentPage = tests.CurrentPage,
                TatalPages = tests.TotalPages,
                HasNextPage = tests.HasNextPage,
                HasPreviousPage = tests.HasPreviousPage,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetTests))).ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetTests))).ToString(),

            };

            var response = new ApiResponse<IEnumerable<TestDto>>(testDtos)
            {
                Meta = metadata
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));

            return Ok(response);
        }

    }
}
