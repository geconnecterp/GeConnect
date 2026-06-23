using gc.api.core.Contratos.Servicios.Gen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.gen.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class GenController : ControllerBase
    {
        private readonly ILogger<GenController> _logger;
        private readonly IGenServicio _genServicio;

        public GenController(ILogger<GenController> logger, IGenServicio genServicio)
        {
            _logger = logger;
            _genServicio = genServicio;
        }

        /// <summary>
        /// Invoca una API externa de forma genérica.
        /// </summary>
        /// <remarks>
        /// Esta acción permite llamar a cualquier API externa, especificando la URL, el método HTTP,
        /// un token de autorización opcional y un cuerpo de solicitud JSON.
        /// </remarks>
        /// <param name="request">Datos de la solicitud para invocar la API externa.</param>
        /// <returns>La respuesta de la API externa.</returns>
        [HttpPost("invoke")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InvokeApi([FromBody] ApiInvokeRequest request)
        {
            _logger.LogInformation("Iniciando el proceso de InvokeApi.");
            RespuestaGenericaBase<string>? response = null;

            if (request == null)
            {
                _logger.LogWarning("La solicitud para InvokeApi es nula.");
                return BadRequest("La solicitud no puede ser nula.");
            }
            if (!ModelState.IsValid || !Uri.TryCreate(request.Url, UriKind.Absolute, out _))
            {
                _logger.LogWarning("La solicitud para InvokeApi es inválida o la URL no tiene el formato correcto. URL: {Url}", request.Url);
                return BadRequest("La solicitud es inválida o la URL no tiene el formato correcto.");
            }
            if (request.Method != "GET" && request.Method != "POST")
            {
                _logger.LogWarning("Método HTTP no válido en InvokeApi: {Method}", request.Method);
                return BadRequest("El método HTTP no es válido. Solo se permiten GET y POST.");
            }

            _logger.LogInformation("Invocando el servicio para el método {Method} en la URL {Url}", request.Method, request.Url);

            if (request.Method == "POST")
            {
                response = await _genServicio.InvokeApiPOST(request);
            }
            else 
            {
                response = await _genServicio.InvokeApiGET(request);
            }

            if (response == null) {
                _logger.LogError("No se pudo procesar la solicitud en InvokeApi para la URL {Url}.", request.Url);
                return BadRequest("No se pudo procesar la solicitud.");
            }

            _logger.LogInformation("Finalizado el proceso de InvokeApi con éxito para la URL {Url}.", request.Url);
            return Ok(response);

        }

        [AllowAnonymous]
        [HttpPost("invoke2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InvokeApi2([FromBody] ApiInvokeRequest request)
        {
            _logger.LogInformation("Iniciando el proceso de InvokeApi2.");
            RespuestaGenericaBase<string>? response = null;

            if (request == null)
            {
                _logger.LogWarning("La solicitud para InvokeApi2 es nula.");
                return BadRequest("La solicitud no puede ser nula.");
            }
            if (!ModelState.IsValid || !Uri.TryCreate(request.Url, UriKind.Absolute, out _))
            {
                _logger.LogWarning("La solicitud para InvokeApi2 es inválida o la URL no tiene el formato correcto. URL: {Url}", request.Url);
                return BadRequest("La solicitud es inválida o la URL no tiene el formato correcto.");
            }
            if (request.Method != "GET" && request.Method != "POST")
            {
                _logger.LogWarning("Método HTTP no válido en InvokeApi2: {Method}", request.Method);
                return BadRequest("El método HTTP no es válido. Solo se permiten GET y POST.");
            }

            _logger.LogInformation("Invocando el servicio para el método {Method} en la URL {Url} (InvokeApi2)", request.Method, request.Url);

            if (request.Method == "POST")
            {
                response = await _genServicio.InvokeApiPOST(request);
            }
            else
            {
                response = await _genServicio.InvokeApiGET(request);
            }

            if (response == null)
            {
                _logger.LogError("No se pudo procesar la solicitud en InvokeApi2 para la URL {Url}.", request.Url);
                return BadRequest("No se pudo procesar la solicitud.");
            }

            _logger.LogInformation("Finalizado el proceso de InvokeApi2 con éxito para la URL {Url}.", request.Url);
            return Ok(response);

        }
    }
}
