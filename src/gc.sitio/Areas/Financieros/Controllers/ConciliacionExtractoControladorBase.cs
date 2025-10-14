using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	public class ConciliacionExtractoControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ConciliacionExtractoControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
		}
		public List<FinancieroDesdeSeleccionDeTipoDto> ListaCuentaBancos
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaCuentaBancos");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<FinancieroDesdeSeleccionDeTipoDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaCuentaBancos", json);
			}
		}
		public List<RegistroExtractoDto> ListaItemsExtracto
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaItemsExtracto");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<RegistroExtractoDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaItemsExtracto", json);
			}
		}

		public List<RegistroSistemaDto> ListaItemsSistema
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaItemsSistema");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<RegistroSistemaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaItemsSistema", json);
			}
		}
	}
}
