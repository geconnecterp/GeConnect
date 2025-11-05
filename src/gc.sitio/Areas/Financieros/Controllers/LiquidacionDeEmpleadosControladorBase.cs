using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Financieros;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	public class LiquidacionDeEmpleadosControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public LiquidacionDeEmpleadosControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<Dictionary<string, object>> ListaTempArchivoParaImportar
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaTempArchivoParaImportar");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaTempArchivoParaImportar", json);
			}
		}

		public List<LiqTopeDto> LiqTopeLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("LiqTopeLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<LiqTopeDto>();
				}
				return JsonConvert.DeserializeObject<List<LiqTopeDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("LiqTopeLista", json);
			}
		}

		public List<LiqEmpleadoEncabezadoDto> LiqEmpleadoEncabezadoLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("LiqEmpleadoEncabezadoLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<LiqEmpleadoEncabezadoDto>();
				}
				return JsonConvert.DeserializeObject<List<LiqEmpleadoEncabezadoDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("LiqEmpleadoEncabezadoLista", json);
			}
		}

		public List<LiqEmpleadoDetalleDto> LiqEmpleadoDetalleLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("LiqEmpleadoDetalleLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<LiqEmpleadoDetalleDto>();
				}
				return JsonConvert.DeserializeObject<List<LiqEmpleadoDetalleDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("LiqEmpleadoDetalleLista", json);
			}
		}
	}
}
