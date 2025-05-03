using AutoMapper;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Servicios;
using gc.api.infra.Datos.Contratos.Security;
using gc.infraestructura.Core.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace gc.api.Controllers.Security
{
    //[Authorize(Roles = "Administrador")] //no se habilita ya que la primera vez no tendriamos token
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiSecurityController : ControllerBase
    {
        private readonly ISecurityServicio _securityServicio;
        private readonly IPasswordService _passwordService;
        private readonly IMenuService _menuService;
        private readonly ILogger<ApiSecurityController> _logger;
        private readonly IMapper _mapper;

        public ApiSecurityController(ISecurityServicio securityServicio, IPasswordService passwordService,IMapper mapper, IMenuService menuService,
            ILogger<ApiSecurityController> logger)
        {
            _securityServicio = securityServicio;
            _passwordService = passwordService;
            _logger = logger;
            _mapper = mapper;
            _menuService = menuService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(RegistroUserDto registroUserDto) //la entidad enviada debe venir en formato Json
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            registroUserDto.Password = _passwordService.CalculaClave(registroUserDto);

            Usuario usuarios = _mapper.Map<Usuario>(registroUserDto);

            var res = await _securityServicio.RegistrerUser(usuarios);

            var response = new ApiResponse<bool>(res);

            return Ok(response);
        }

        [HttpGet]
		[Route("[action]")]
		public IActionResult ObtenerMenuPorUsuario(string usuId)
		{
			_logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var respuesta = _menuService.GetMenuList(usuId);
			var response = new ApiResponse<List<UsuarioMenu>>(respuesta);

			return Ok(response);
		}
	}
}
