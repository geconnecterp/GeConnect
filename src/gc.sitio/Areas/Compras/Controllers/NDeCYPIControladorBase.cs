using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers
{
	public class NDeCYPIControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public NDeCYPIControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<ProductoNCPIDto> ListaProductoNCPI
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaProductoNCPI");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<ProductoNCPIDto>();
				}
				return JsonConvert.DeserializeObject<List<ProductoNCPIDto>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaProductoNCPI", valor);
			}
		}

		public List<AdministracionDto> ListaSucursales
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaSucursales");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<AdministracionDto>();
				}
				return JsonConvert.DeserializeObject<List<AdministracionDto>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaSucursales", valor);
			}
		}

		public List<DepositoDto> ListaDepositos
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaDepositos");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<DepositoDto>();
				}
				return JsonConvert.DeserializeObject<List<DepositoDto>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaDepositos", valor);
			}
		}
	}
}
