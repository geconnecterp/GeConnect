using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers
{
    public class ComprobanteDeCompraControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ILogger _logger;
		public ComprobanteDeCompraControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_logger = logger;
		}

		public List<ConceptoFacturadoDto> ListaConceptoFacturado
		{
			get
			{
				var txt = _context.HttpContext.Session.GetString("ListaConceptoFacturado");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<ConceptoFacturadoDto>();
				}
				return JsonConvert.DeserializeObject<List<ConceptoFacturadoDto>>(txt); ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("ListaConceptoFacturado", valor);
			}

		}

		public List<OtroTributoDto> ListaOtroTributo
		{
			get
			{
				var txt = _context.HttpContext.Session.GetString("ListaOtroTributo");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<OtroTributoDto>();
				}
				return JsonConvert.DeserializeObject<List<OtroTributoDto>>(txt); ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("ListaOtroTributo", valor);
			}

		}

		public List<OrdenDeCompraConceptoDto> ListaTotales
		{
			get
			{
				var txt = _context.HttpContext.Session.GetString("ListaTotales");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<OrdenDeCompraConceptoDto>();
				}
				return JsonConvert.DeserializeObject<List<OrdenDeCompraConceptoDto>>(txt); ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("ListaTotales", valor);
			}

		}
	}
}
