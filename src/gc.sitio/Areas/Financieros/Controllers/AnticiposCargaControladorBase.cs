using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Financieros;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	public class AnticiposCargaControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public AnticiposCargaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<CuentaDto> ProveedoresLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ProveedoresLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<CuentaDto>();
				}
				return JsonConvert.DeserializeObject<List<CuentaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ProveedoresLista", json);
			}
		}

		public List<CuentaDto> ClientesLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ClientesLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<CuentaDto>();
				}
				return JsonConvert.DeserializeObject<List<CuentaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ClientesLista", json);
			}
		}

		public List<AnticipoDto> AnticiposLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("AnticiposLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<AnticipoDto>();
				}
				return JsonConvert.DeserializeObject<List<AnticipoDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("AnticiposLista", json);
			}
		}

		public CuentaDto ProveedorDefault
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ProveedorDefault") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new CuentaDto();
				}
				return JsonConvert.DeserializeObject<CuentaDto>(json) ?? new CuentaDto();
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ProveedorDefault", json);
			}
		}
	}
}
