using gc.api.core.Contratos.Servicios;
using gc.api.core.Interfaces.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.Dtos.Users.Request;
using gc.infraestructura.Dtos.Seguridad;
using log4net.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

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
        private readonly ISecurityServicio _securityServicio;
        private readonly IConfiguration _configuration;


        public UsuariosController(ILogger<UsuariosController> logger, IHttpContextAccessor httpContext,
            IApiUsuarioServicio usuSv, IUriService uriService, ISecurityServicio securityServicio,
            IConfiguration configuration)
        {
            _context = httpContext;
            _logger = logger;
            _usuSv = usuSv;
            _uriService = uriService;
            _securityServicio = securityServicio;
            _configuration = configuration;
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

		[HttpPost]
		[Route("[action]")]
		public IActionResult BuscarUsuariosParaLista(BuscarUsuarioRequest filtro)
		{
			if (filtro == null)
			{
				return BadRequest("No se recepcionaron datos para realizar la busqueda.");
			}

            var user = _usuSv.BuscarUsuarios(filtro);
			return Ok(new ApiResponse<List<UserDto>>(user));
		}
		[HttpGet]
		[Route("[action]")]
		public IActionResult BuscarUsuarioLista(string adm_id)
		{
			if (string.IsNullOrEmpty(adm_id))
			{
				return BadRequest("No se proporcionó el usuario.");
			}
			var user = _usuSv.BuscarUsuarioLista(adm_id);
			return Ok(new ApiResponse<List<UserDto>>(user));
		}

        [HttpGet]
        [Route("[action]")]
        public IActionResult OperacionesSeguridadDisponibles()
        {
            var usuario = ObtenerUsuarioAutenticado();
            return Ok(new ApiResponse<OperacionesSeguridadUsuarioDto>(
                _securityServicio.ObtenerOperacionesSeguridad(usuario)));
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult BlanquearClave(OperacionUsuarioSeguridadRequestDto request)
        {
            var ejecutor = ObtenerUsuarioAutenticado();
            if (!_securityServicio.ObtenerOperacionesSeguridad(ejecutor).PuedeBlanquearClave)
                return Forbid();

            string objetivo = request?.UsuarioObjetivo?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(objetivo) || objetivo.Length > 10)
                return BadRequest("El usuario seleccionado no es válido.");
            if (string.Equals(objetivo, ejecutor, StringComparison.OrdinalIgnoreCase))
                return BadRequest("No puede blanquear su propia contraseña.");

            string claveTemporal = _configuration["SecurityOperations:TemporaryPassword"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(claveTemporal))
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    "La contraseña temporal no está configurada en el servidor.");

            var resultado = _securityServicio.BlanquearClave(objetivo, ejecutor, claveTemporal,
                ObtenerAdministracion(), ObtenerIp(), Guid.NewGuid());
            return Ok(new ApiResponse<CambioClaveResultadoDto>(resultado));
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult DesbloquearUsuario(OperacionUsuarioSeguridadRequestDto request)
        {
            var ejecutor = ObtenerUsuarioAutenticado();
            if (!_securityServicio.ObtenerOperacionesSeguridad(ejecutor).PuedeDesbloquearUsuario)
                return Forbid();

            string objetivo = request?.UsuarioObjetivo?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(objetivo) || objetivo.Length > 10)
                return BadRequest("El usuario seleccionado no es válido.");
            if (string.Equals(objetivo, ejecutor, StringComparison.OrdinalIgnoreCase))
                return BadRequest("No puede desbloquear su propio usuario.");

            var resultado = _securityServicio.DesbloquearUsuario(objetivo, ejecutor,
                ObtenerAdministracion(), ObtenerIp(), Guid.NewGuid());
            return Ok(new ApiResponse<CambioClaveResultadoDto>(resultado));
        }

        private string ObtenerUsuarioAutenticado()
        {
            return User.FindFirst("user")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("No fue posible identificar al usuario autenticado.");
        }

        private string? ObtenerAdministracion() => User.FindFirst("AdmId")?.Value?.Split('#', 2)[0];

        private string? ObtenerIp() => Request.Headers["X-ClientUsr"].FirstOrDefault()
            ?? HttpContext.Connection.RemoteIpAddress?.ToString();
	}
}
