using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Servicios.Libros;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.CodeDom;

namespace gc.api.Controllers.Libros
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiBalanceGrController : ControllerBase
    {
        private readonly IApiBalanceGeneralServicio _balGrSv;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ApiBalanceGrController> _logger;

        public ApiBalanceGrController(
            IApiBalanceGeneralServicio balGrSv,
            IOptions<AppSettings> appSettings,
            ILogger<ApiBalanceGrController> logger)
        {
            _balGrSv = balGrSv;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ApiResponse<List<BalanseGrDto>>> ObtenerLibroMayor(int eje_nro)
        {
            // Validar parámetros de entrada            
            // Validación específica para asientos definitivos: el ejercicio es obligatorio
            if (eje_nro <= 0)
            {
                return BadRequest(new ApiResponse<string>("Debe seleccionar un ejercicio contable para consultar el Balance General."));
            }
            // Llamar al servicio para obtener los asientos
            var resultado = _balGrSv.ObtenerBalanceGeneral(eje_nro);
           if(resultado.Count == 0)
            {
                throw new NegocioException("No se encontraron registros para el ejercicio especificado.");
            }
            var response = new ApiResponse<List<BalanseGrDto>>(resultado);
            

            return Ok(response);
        }
    }
}
