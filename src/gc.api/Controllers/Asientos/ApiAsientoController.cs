using gc.api.core.Contratos.Servicios.Asientos;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
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
                return Ok(new ApiResponse<List<EjercicioDto>>(resultado));
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

                return Ok(new ApiResponse<List<TipoAsientoDto>>(resultado));
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
                return Ok(new ApiResponse<List<UsuAsientoDto>>(resultado));
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el detalle de los asientos de ajuste por inflación para un ejercicio contable específico
        /// </summary>
        /// <param name="eje_nro">Número de ejercicio</param>
        /// <returns>Lista de usuarios del ejercicio</returns>
        [HttpGet("asiento-ajuste/{eje_nro}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<AsientoAjusteDto>> ObtenerAsientosAjuste(int eje_nro)
        {
            if (eje_nro <= 0)
            {
                return BadRequest("El número de ejercicio debe ser mayor a cero");
            }

            try
            {
                var resultado = _asientosServicio.ObtenerAsientosAjuste(eje_nro);
                return Ok(new ApiResponse<List<AsientoAjusteDto>>(resultado));
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el detalle de los asientos de ajuste por inflación para un ejercicio contable específico
        /// </summary>
        /// <param name="eje_nro">Número de ejercicio</param>
        /// <param name="ccb_id">Id de la cuenta contable</param>
        /// <param name="todas">Indica si se deben obtener todos los asientos o solo los de la cuenta contable especificada</param>
        /// <returns>Lista de usuarios del ejercicio</returns>
        [HttpGet("asiento-ajuste-ccb/{eje_nro}/{ccb_id}/{todas}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<AsientoAjusteCcbDto>> ObtenerAsientosAjusteCcb(int eje_nro, string ccb_id, bool todas)
        {
            if (eje_nro <= 0)
            {
                return BadRequest("El número de ejercicio debe ser mayor a cero");
            }

            try
            {
                var resultado = _asientosServicio.ObtenerAsientosAjusteCcb(eje_nro, ccb_id, todas);
                return Ok(new ApiResponse<List<AsientoAjusteCcbDto>>(resultado));
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Confirma un asiento de ajuste por inflación
        /// </summary>
        /// <param name="confirmar">Datos para confirmar el asiento de ajuste</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("confirmar-asiento-ajuste")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<RespuestaDto> ConfirmarAsientoAjuste([FromBody] AjusteConfirmarDto confirmar)
        {
            if (confirmar == null)
            {
                return BadRequest("Los datos para confirmar el asiento no pueden ser nulos");
            }

            if (confirmar.EjeNro <= 0)
            {
                return BadRequest("El número de ejercicio debe ser mayor a cero");
            }

            if (string.IsNullOrEmpty(confirmar.User))
            {
                return BadRequest("El identificador de usuario es requerido");
            }

            if (string.IsNullOrEmpty(confirmar.Json))
            {
                return BadRequest("El detalle del asiento (Json) es requerido");
            }

            var resultado = _asientosServicio.ConfirmarAsientoAjuste(confirmar);

            return Ok(new ApiResponse<RespuestaDto>(resultado));
        }
    }
}
