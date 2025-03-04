using gc.api.Controllers.Users;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Consultas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Consultas
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultaCCController : ControllerBase
    {
        private readonly ILogger<ConsultaCCController> _logger;
        private readonly IHttpContextAccessor _context;
        private readonly IConsultaServicio _consSv;

        public ConsultaCCController(ILogger<ConsultaCCController> logger, IHttpContextAccessor accessor, 
            IConsultaServicio consulta)
        {
            _logger = logger;
            _context = accessor;
            _consSv = consulta;
        }

        [HttpGet]
        public IActionResult ConsultarCuentaCorriente(string ctaId, long fechaD, string userId)
        {
            if (string.IsNullOrEmpty(ctaId))
            {
                return BadRequest("No se recepcionó ninguna cuenta");
            }
            if (fechaD == 0)
            {
                return BadRequest("No se ha especificado el periodo DESDE");
            }
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se recepcionó el usuario.");
            }

            var fd = new DateTime(fechaD);

            var regs = _consSv.ConsultarCuentaCorriente(ctaId, fd, userId);
            return Ok(new ApiResponse<List<ConsCtaCteDto>>(regs));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ConsultaVencimientoComprobantesNoImputados(string ctaId, long fechaD, long fechaH, string userId)
        {
            if (string.IsNullOrEmpty(ctaId))
            {
                return BadRequest("No se recepcionó ninguna cuenta");
            }
            if (fechaD == 0)
            {
                return BadRequest("No se ha especificado la fecha DESDE");
            }
            if (fechaH == 0)
            {
                return BadRequest("No se ha especificado la fecha hasta");
            }
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se recepcionó el usuario.");
            }

            var fd = new DateTime(fechaD);
            var fh = new DateTime(fechaH);

            var regs = _consSv.ConsultaVencimientoComprobantesNoImputados(ctaId, fd, fh, userId);
            return Ok(new ApiResponse<List<ConsVtoDto>>(regs));
        }
        [HttpGet]
        [Route("[action]")]
        public IActionResult ConsultaComprobantesMeses(string ctaId, int meses, bool relCuit, string userId)
        {
            if (string.IsNullOrEmpty(ctaId))
            {
                return BadRequest("No se recepcionó ninguna cuenta");
            }
            if (meses < 1 || meses >60)
            {
                return BadRequest("No se ha especificado, correctamente, la cantidad meses. Se pueden especificar hasta 60 meses.");
            }
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se recepcionó el usuario.");
            }          

            var regs = _consSv.ConsultaComprobantesMeses(ctaId, meses,relCuit, userId);
            return Ok(new ApiResponse<List<ConsCompTotDto>>(regs));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ConsultaComprobantesMesDetalle(string ctaId, string mes, bool relCuit, string userId)
        {
            if (string.IsNullOrEmpty(ctaId))
            {
                return BadRequest("No se recepcionó ninguna cuenta");
            }
            if (mes.Length != 6 && mes.ToIntOrNull()==null)
            {
                return BadRequest("No se ha especificado el mes");
            }
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se recepcionó el usuario.");
            }

            

            var regs = _consSv.ConsultaComprobantesMesDetalle(ctaId, mes,relCuit, userId);
            return Ok(new ApiResponse<List<ConsCompDetDto>>(regs));
        }
    }
}
