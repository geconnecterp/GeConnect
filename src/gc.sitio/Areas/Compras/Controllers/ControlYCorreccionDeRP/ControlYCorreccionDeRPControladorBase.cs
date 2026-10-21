using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers.ControlYCorreccionDeRP
{
	public class ControlYCorreccionDeRPControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ControlYCorreccionDeRPControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<RprCtlPendienteDto> RprCtlPendienteLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("RprCtlPendienteLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<RprCtlPendienteDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("RprCtlPendienteLista", json);
			}
		}

		public List<RprCtlItemDetalleDto> RprCtlDetalleLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("RprCtlDetalleLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<RprCtlItemDetalleDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("RprCtlDetalleLista", json);
			}
		}
	}
}
