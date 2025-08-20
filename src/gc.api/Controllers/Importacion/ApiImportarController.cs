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

        [HttpPost("cargar-perfil-precio")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RespuestaCPDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult CargarImportacionPrecioPerfil(AbmGenDto cargarPerfil)
        {
            if (cargarPerfil == null || 
                string.IsNullOrEmpty(cargarPerfil.Objeto) || 
                string.IsNullOrEmpty(cargarPerfil.Usuario) ||
                string.IsNullOrEmpty(cargarPerfil.Administracion) || 
                string.IsNullOrEmpty(cargarPerfil.Json))
            {
                return BadRequest("Los datos del perfil de precios son inválidos.");
            }
            List<RespuestaCPDto> resultado = _importarServicio.CargarImportacionPrecioPerfil(
                cargarPerfil.Objeto, 
                cargarPerfil.Usuario, 
                cargarPerfil.Administracion, 
                cargarPerfil.Json);

            
            return Ok(new ApiResponse<List<RespuestaCPDto>>(resultado));
        }
    }
}
