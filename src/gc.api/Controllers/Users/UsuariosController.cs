using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Users;
using log4net.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace gc.api.Controllers.Users
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ILogger<UsuariosController> _logger;
        private readonly IHttpContextAccessor _context;
        private readonly IApiUsuarioServicio _usuSv;
        private readonly IUriService _uriService;


        public UsuariosController(ILogger<UsuariosController> logger, IHttpContextAccessor httpContext, IApiUsuarioServicio usuSv,IUriService uriService)
        {
            _context = httpContext;
            _logger = logger;
            _usuSv = usuSv;
            _uriService = uriService;
        }

        /// <summary>
        /// Busqueda por filtro de usuarios
        /// </summary>
        /// <param name="filtro"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public IActionResult BuscarUsuarios(QueryFilters filtro)
        {
            UserDto reg = new UserDto();
            if (filtro == null)
            {
                return BadRequest("No se recepcionó el filtro de la busqueda de Usuarios.");
            }

            var res = _usuSv.BuscarUsuarios(filtro);
            if (res.Count > 0)
            {
                reg = res.First();
            }
            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = reg.Total_registros,
                PageSize = filtro.Registros??0,
                CurrentPage = filtro.Pagina??0,
                TotalPages = reg.Total_paginas,
                HasNextPage = (filtro.Pagina ?? 0) < reg.Total_paginas,
                HasPreviousPage = (filtro.Pagina ?? 0) > 1,
                NextPageUrl = _uriService.GetPostPaginationUri(filtro, Url.RouteUrl(nameof(BuscarUsuarios)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filtro, Url.RouteUrl(nameof(BuscarUsuarios)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<UserDto>>(res)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult BuscarUsuarioDatos(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se recepcionó el usuario a buscar");
            }
            var user = _usuSv.BuscarUsuarioDatos(userId);
            return Ok(new ApiResponse<UserDto>(user));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerPerfilesDelUsuario(string userId) {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se recepcionó el usuario para presentar las Administraciones a las cuales tiene acceso.");
            }
            var user = _usuSv.ObtenerPerfilesDelUsuario(userId);
            return Ok(new ApiResponse<List<PerfilUserDto>>(user));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerAdministracionesDelUsuario(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se recepcionó el usuario para presentar las Administraciones a las cuales tiene acceso.");
            }
            var user = _usuSv.ObtenerAdministracionesDelUsuario(userId);
            return Ok(new ApiResponse<List<AdmUserDto>>(user));
        }
        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerDerechosDelUsuario(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se recepcionó el usuario para presentar las Administraciones a las cuales tiene acceso.");
            }
            var user = _usuSv.ObtenerDerechosDelUsuario(userId);
            return Ok(new ApiResponse<List<DerUserDto>>(user));
        }
    }
}
