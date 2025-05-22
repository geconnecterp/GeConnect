using gc.api.core.Contratos.Servicios.Asientos;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Asientos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Asientos
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ApiAsientoController : ControllerBase
    {
        private readonly IAsientoServicio _asientosServicio;

        public ApiAsientoController(IAsientoServicio asientosServicio)
        {
            _asientosServicio = asientosServicio;
        }

        /// <summary>
        /// Obtiene la lista de ejercicios contables disponibles
        /// </summary>
        /// <returns>Lista de ejercicios</returns>
        [HttpGet("ejercicios")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<EjercicioDto>> ObtenerEjercicios()
        {
            try
            {
                var resultado = _asientosServicio.ObtenerEjercicios();
                return Ok(new ApiResponse<List<EjercicioDto>>( resultado));
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los tipos de asientos disponibles
        /// </summary>
        /// <returns>Lista de tipos de asiento</returns>
        [HttpGet("tipos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<TipoAsientoDto>> ObtenerTiposAsiento()
        {
            try
            {
                var resultado = _asientosServicio.ObtenerTiposAsiento();
                
                return Ok(new ApiResponse<List<TipoAsientoDto>>( resultado));
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los usuarios asociados a un ejercicio contable específico
        /// </summary>
        /// <param name="eje_nro">Número de ejercicio</param>
        /// <returns>Lista de usuarios del ejercicio</returns>
        [HttpGet("usuarios-ejercicio/{eje_nro}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<UsuAsientoDto>> ObtenerUsuariosDeEjercicio(int eje_nro)
        {
            if (eje_nro < 0)
            {
                return BadRequest("El número de ejercicio debe ser mayor o igual a cero");
            }

            try
            {
                var resultado = _asientosServicio.ObtenerUsuariosDeEjercicio(eje_nro);
                return Ok(new ApiResponse<List<UsuAsientoDto>>( resultado));
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
