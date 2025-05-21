using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers
{
    public class OrdenDeCompraConsultaControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ILogger _logger;
		public OrdenDeCompraConsultaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_logger = logger;
		}

		public List<OrdenDeCompraConsultaDto> ListaOrdenDeCompraConsulta
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaOrdenDeCompraConsulta");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<OrdenDeCompraConsultaDto>();
				}
				return JsonConvert.DeserializeObject<List<OrdenDeCompraConsultaDto>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaOrdenDeCompraConsulta", valor);
			}

		}
	}
}
