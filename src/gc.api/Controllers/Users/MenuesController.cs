using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace gc.api.Controllers.Users
{

    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuesController : ControllerBase
    {
        private  IApiUsuarioServicio _usuSv;
        private readonly IUriService _uriService;

        public MenuesController(IApiUsuarioServicio usuSv, IUriService uriService)
        {
            _usuSv = usuSv;
            _uriService = uriService;

        }


        /// <summary>
        /// Obtener los Perfiles de Usuario del sistema
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public IActionResult GetPerfiles(QueryFilters filters)
        {
            PerfilDto reg = new PerfilDto();
            if (filters == null)
            {
                return BadRequest("No se recepcionaron datos para la consulta de Perfiles");
            }
            var res = _usuSv.GetPerfiles(filters);

            if (res.Count > 0)
            {
                reg = res.First();
            }

            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = reg.Total_Registros,
                PageSize = filters.Registros.Value,
                CurrentPage = filters.Pagina.Value,
                TotalPages = reg.Total_Paginas,
                HasNextPage = filters.Pagina.Value < reg.Total_Paginas,
                HasPreviousPage = filters.Pagina.Value > 1,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetPerfiles)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetPerfiles)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<PerfilDto>>(res)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        /// <summary>
        /// en esta acción se procedera a devolver el perfil que se pretende presentar en pantalla
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public IActionResult GetPerfil(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("No se recepcionó el identificador del perfil a Observar");
            }

            var res = _usuSv.GetPerfil(id);
            return Ok(new ApiResponse<PerfilDto>(res));

        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetUsuariosxPerfiles(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("No se recepcionó el identificador del perfil a Analizar");
            }

            var res = _usuSv.GetPerfilUsers(id);
            return Ok(new ApiResponse<List<PerfilUserDto>>(res));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetMenu()
        {
            var res = _usuSv.GetMenu();
            return Ok(new ApiResponse<List<MenuDto>>(res));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetMenuItems(string menuId, string perfil)
        {
            if (string.IsNullOrEmpty(menuId))
            {
                return BadRequest("No se recepcionó el identificador del menú.");
            }

            if (string.IsNullOrEmpty(perfil))
            {
                return BadRequest("No se recepcionó el Perfil para el menú.");
            }

            var res = _usuSv.GetMenuItems(menuId,perfil);
            return Ok(new ApiResponse<List<MenuItemsDto>>(res));
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult DefinePerfilDefault(PerfilUserDto perfil)
        {
            if (string.IsNullOrEmpty(perfil.perfil_id))
            {
                return BadRequest("No se recepcionó el perfil.");
            }

            if (string.IsNullOrEmpty(perfil.usu_id))
            {
                return BadRequest("No se recepcionó el usuario.");
            }

            var res = _usuSv.DefinePerfilDefault(perfil);
            return Ok(new ApiResponse<RespuestaDto>(res));
        }


        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerMenu(string perfilId, string user, string menuId, string adm)
        {
            if (string.IsNullOrEmpty(perfilId))
            {
                return BadRequest("No se recepcionó el perfil.");
            }

            if (string.IsNullOrEmpty(user))
            {
                return BadRequest("No se recepcionó el usuario.");
            }

            if (string.IsNullOrEmpty(menuId))
            {
                return BadRequest("No se recepcionó que menú presentar.");
            }

            if (string.IsNullOrEmpty(adm))
            {
                return BadRequest("No se recepcionó la Administración.");
            }
            var res = _usuSv.ObtenerMenu(perfilId,user,menuId,adm);
            return Ok(new ApiResponse<List<MenuPpalDto>>(res));
        }
    }
}
