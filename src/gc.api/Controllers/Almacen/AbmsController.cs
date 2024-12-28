using gc.api.core.Contratos.Servicios.ABM;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Almacen
{

    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class AbmsController : ControllerBase
    {
        private readonly IAbmServicio _abmSv;
        private readonly ILogger<AbmsController> _logger;
        public AbmsController(IAbmServicio abmServicio, ILogger<AbmsController> logger)
        {
            _abmSv = abmServicio;
            _logger = logger;
        }
        [HttpPost]
        public IActionResult GrabarAbm(AbmGenDto abmGen)
        {
            var abm = new List<char>() { 'A', 'B', 'M' };
            if (abmGen == null)
            {
                return BadRequest("No se recepcionó la información a preservar.");
            }
            if (string.IsNullOrEmpty(abmGen.Objeto))
            {
                return BadRequest("No se recepcionó sobre que módulo se desea realizar la preservación de datos.");
            }
            if (!abm.Contains(abmGen.Abm))
            {
                return BadRequest("Se desconoce que operación desea realizar.");
            }
            if (string.IsNullOrEmpty(abmGen.Usuario))
            {
                return BadRequest("No se recepcionó el usuario que está llevando adelante la operación.");
            }
            if (string.IsNullOrEmpty(abmGen.Administracion))
            {
                return BadRequest("No se recepcionó la Administración a la que pertenece la operación.");
            }

            var res = _abmSv.ConfirmarABM(abmGen);

            return Ok(new ApiResponse<RespuestaDto>(res));


        }
    }
}
