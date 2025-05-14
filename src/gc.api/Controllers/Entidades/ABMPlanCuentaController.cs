using gc.api.core.Contratos.Servicios.Contable;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace gc.api.Controllers.Entidades
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ABMPlanCuentaController : ControllerBase
    {
        private readonly ILogger<ABMPlanCuentaController> _logger;
        private readonly IABMPlanCuentaServicio _pcuentaServicio;
        private readonly IUriService _uriService;

        public ABMPlanCuentaController(ILogger<ABMPlanCuentaController> logger, IABMPlanCuentaServicio pcSv,
            IUriService uriService)
        {
            _logger = logger;
            _pcuentaServicio = pcSv;
            _uriService = uriService;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<PlanCuentaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ObtenerPlanCuentas( QueryFilters filters)
        {
            var reg = new PlanCuentaDto();// { Total_Paginas = 0, Total_Registros = 0 };
            var lista = _pcuentaServicio.ObtenerPlanCuenta(filters);

            if (lista.Count > 0)
            {
                reg = lista.First();
            }
            else
            {
                return NotFound("No se encontraron PlanCuentaes");
            }        

            var response = new ApiResponse<List<PlanCuentaDto>>(lista)
            {
                //Meta = metadata
            };

            //Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        [HttpGet]
        public IActionResult ObtenerPlanCuenta(string ccb_id)
        {
            if (string.IsNullOrEmpty(ccb_id))
            {
                return BadRequest("El id de la Cuenta no puede ser nulo");
            }
            if (ccb_id.Length > 8)
            {
                return BadRequest("El id de la Cuenta no puede tener más de 8 dígitos");
            }
            if (ccb_id.Length < 8)
            {
                return BadRequest("El id de la Cuenta no puede tener menos de 8 dígitos");
            }
            if (!ccb_id.All(char.IsDigit))
            {
                return BadRequest("El id de la Cuenta solo puede contener números");
            }

            var res = _pcuentaServicio.ObtenerCuenta(ccb_id);
            if (res == null)
            {
                return NotFound("No se encontró la Cuenta solicitada");
            }
            else
            {
                return Ok(new ApiResponse<PlanCuentaDto>(res));
            }
        }
    }
}
