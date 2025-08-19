using gc.api.Controllers.Almacen;
using gc.api.core.Contratos.Servicios.Importacion;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Importacion;
using gc.infraestructura.Dtos.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace gc.api.Controllers.Importacion
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiImportarController : ControllerBase
    {
        private readonly ILogger<ApiImportarController> _logger;
        private readonly IApiImportarServicio _importarServicio;
        public ApiImportarController(ILogger<ApiImportarController> logger, IApiImportarServicio importarServicio)
        {
            _logger = logger;
            _importarServicio = importarServicio;
        }

        [HttpGet("precio-file-dato")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<PrecioFileDatos>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ObtenerPrecioFileDatos()
        {
            
            var resultado = _importarServicio.ObtenerPrecioFileDatos();

            if (resultado == null)
            {
                return BadRequest("No se pudo obtener el listado de los datos de referencia para la importación de listas de precio. Verifique los datos ingresados.");
            }

            return Ok(new ApiResponse<List<PrecioFileDatos>>(resultado));
        }

        [HttpGet("precio-file-perfil")]
        public IActionResult ObtenerPerfildePreciosCliente(string ctaId)
        {
            if(string.IsNullOrEmpty(ctaId))
            {
                return BadRequest("El ID de cliente no puede estar vacío.");
            }
            List<ProveedorPerfilDB> resultado = _importarServicio.ObtenerPerfildePreciosCliente(ctaId);

            return Ok(new ApiResponse<List<ProveedorPerfilDB>>(resultado));
        }

        [HttpPost("confirmar-perfil-precio")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ConfirmarPerfilPrecioPerfil(AbmGenDto confirmarPerfil)
        {
            if (confirmarPerfil == null || 
                string.IsNullOrEmpty(confirmarPerfil.Objeto) || 
                string.IsNullOrEmpty(confirmarPerfil.Usuario) ||
                string.IsNullOrEmpty(confirmarPerfil.Administracion) || 
                string.IsNullOrEmpty(confirmarPerfil.Json))
            {
                return BadRequest("Los datos del perfil de precios son inválidos.");
            }
            RespuestaDto resultado = _importarServicio.ConfirmarPerfilPrecioPerfil(
                confirmarPerfil.Objeto, 
                confirmarPerfil.Usuario, 
                confirmarPerfil.Administracion, 
                confirmarPerfil.Json);
            if (resultado == null)
            {
                return BadRequest("No se pudo confirmar el perfil de precios. Verifique los datos ingresados.");
            }
            return Ok(new ApiResponse<RespuestaDto>(resultado));
        }

    }
}
