using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	public class ChequePagaAcaControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ChequePagaAcaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
		}

		public bool CambioDeFechaDePresentacion
		{
			get
			{
				var bol = _context.HttpContext?.Session.GetString("CambioDeFechaDePresentacion");
				if (string.IsNullOrEmpty(bol) || string.IsNullOrWhiteSpace(bol))
				{
					return false;
				}
				return JsonConvert.DeserializeObject<bool>(bol);
			}
			set
			{
				var bol = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("CambioDeFechaDePresentacion", bol);
			}
		}

		public bool DocumentoEnCuenta
		{
			get
			{
				var bol = _context.HttpContext?.Session.GetString("DocumentoEnCuenta");
				if (string.IsNullOrEmpty(bol) || string.IsNullOrWhiteSpace(bol))
				{
					return false;
				}
				return JsonConvert.DeserializeObject<bool>(bol);
			}
			set
			{
				var bol = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("DocumentoEnCuenta", bol);
			}
		}

		public List<FinancieroCarteraDto> ListaFinancieroCartera
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaFinancieroCartera");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<FinancieroCarteraDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaFinancieroCartera", json);
			}
		}

		public List<FinancieroDesdeSeleccionDeTipoDto> ListaFinancieroDesdeSeleccionDeTipo
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaFinancieroDesdeSeleccionDeTipo");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<FinancieroDesdeSeleccionDeTipoDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaFinancieroDesdeSeleccionDeTipo", json);
			}
		}
	}
}
