using gc.api.Controllers.Almacen;
using gc.api.core.Contratos.Servicios.Importacion;
using gc.api.core.Entidades;
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
        public IActionResult ObtenerPerfilDeProveedor(string ctaId)
        {
            if(string.IsNullOrEmpty(ctaId))
            {
                return BadRequest("El ID de cliente no puede estar vacío.");
            }
           
            List<MapeoColumnaDto> resultado = _importarServicio.ObtenerPerfilDeProveedor(ctaId);

            return Ok(new ApiResponse<List<MapeoColumnaDto>>(resultado));
        }

        [HttpPost("cargar-perfil-precio")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RespuestaCPDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult CargarImportacionPrecioPerfil(AbmPlusGenDto req)
        {
            RespuestaDto respPerfil = new();

            if (req == null || 
                string.IsNullOrEmpty(req.Objeto) || 
                string.IsNullOrEmpty(req.Usuario) ||
                string.IsNullOrEmpty(req.Administracion) || 
                string.IsNullOrEmpty(req.Json))
            {
                return BadRequest("Los datos del perfil de precios son inválidos.");
            }

            //pongo el try catch para que si falla la carga del perfil, igual intente cargar los precios
            try
            {
                respPerfil = _importarServicio.CargaPerfilCuenta(ctaId: req.Objeto, usu: req.Usuario, adm: req.Administracion, json: req.Json2);
                _logger.LogInformation("Carga del perfil de precios realizada correctamente para el proveedor {CtaId}", req.Objeto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Problemas para la carga del perfil");
                
            }

            List<RespuestaCPDto> resultado = _importarServicio.CargarImportacionPrecioPerfil(req);

            
            return Ok(new ApiResponse<List<RespuestaCPDto>>(resultado));
        }
    }
}
