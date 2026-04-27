using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Ventas;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Ventas.Controllers
{
	public class VentasCajasCorreccionCustodiaControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public VentasCajasCorreccionCustodiaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<VtasPVCtlEntregaDto> VtasPVCtlEntregaLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("VtasPVCtlEntregaLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<VtasPVCtlEntregaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("VtasPVCtlEntregaLista", json);
			}
		}

		public List<VtasPVCtlEntregaRendDto> VtasPVCtlEntregaRendLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("VtasPVCtlEntregaRendLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<VtasPVCtlEntregaRendDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("VtasPVCtlEntregaRendLista", json);
			}
		}
	}
}
