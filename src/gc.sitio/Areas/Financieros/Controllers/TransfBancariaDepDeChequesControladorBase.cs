using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	public class TransfBancariaDepDeChequesControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public TransfBancariaDepDeChequesControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
		}

		public List<ValoresDesdeObligYCredDto> OPValoresOrigen
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("OPValoresOrigen");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ValoresDesdeObligYCredDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("OPValoresOrigen", json);
			}
		}

		public List<ValoresDesdeObligYCredDto> OPValoresDestino
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("OPValoresDestino");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ValoresDesdeObligYCredDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("OPValoresDestino", json);
			}
		}
	}
}
