using gc.api.core.Interfaces.Servicios;
using gc.api.infra.Datos.Contratos.Security;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace gc.api.Controllers.Security
{
    //[Authorize(Roles = "Administrador")] //no se habilita ya que la primera vez no tendriamos token
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly ISecurityServicio _securityServicio;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<SecurityController> _logger;

        public SecurityController(ISecurityServicio securityServicio, IPasswordService passwordService, ILogger<SecurityController> logger)
        {
            _securityServicio = securityServicio;
            _passwordService = passwordService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(RegistroUserDto registroUserDto) //la entidad enviada debe venir en formato Json
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            registroUserDto.Password = _passwordService.Hash(registroUserDto.Password);
            var res = await _securityServicio.RegistrerUser(registroUserDto);

            var response = new ApiResponse<bool>(res);

            return Ok(response);
        }
    }
}
