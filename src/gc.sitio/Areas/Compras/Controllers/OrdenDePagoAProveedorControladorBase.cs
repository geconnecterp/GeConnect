using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers
{
	public class OrdenDePagoAProveedorControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ILogger _logger;
		public OrdenDePagoAProveedorControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_logger = logger;
		}

		public string CtaIdSelected
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("CtaIdSelected");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return string.Empty;
				}
				return JsonConvert.DeserializeObject<string>(txt); ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("CtaIdSelected", valor);
			}

		}

		public List<OPValidacionPrevDto> OPValidacionPrevLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("OPValidacionPrevLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<OPValidacionPrevDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("OPValidacionPrevLista", json);
			}
		}
	}
}
