using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Users;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Users
{

    //[Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuesController : ControllerBase
    {
        private  IApiUsuarioServicio _usuSv;

        public MenuesController(IApiUsuarioServicio usuSv)
        {
            _usuSv = usuSv;
        }


        /// <summary>
        /// Obtener los Perfiles de Usuario del sistema
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public IActionResult GetPerfiles(QueryFilters filters)
        {
            if (filters == null)
            {
                return BadRequest("No se recepcionaron datos para la consulta de Perfiles");
            }
            var res = _usuSv.GetPerfiles(filters);
            return Ok(new ApiResponse<List<PerfilDto>>(res));
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
    }
}
