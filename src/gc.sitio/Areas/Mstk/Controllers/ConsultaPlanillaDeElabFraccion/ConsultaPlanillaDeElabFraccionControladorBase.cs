using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Mstk;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Mstk.Controllers.ConsultaPlanillaDeElabFraccion
{
	public class ConsultaPlanillaDeElabFraccionControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ConsultaPlanillaDeElabFraccionControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<PlanillaElabYFraccionListaDto> ListaplanillaElabYFraccion
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaplanillaElabYFraccion");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<PlanillaElabYFraccionListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaplanillaElabYFraccion", json);
			}
		}

		public MetadataGrid MetadataListaPlanillaElabYFraccion
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataListaPlanillaElabYFraccion");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt);
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataListaPlanillaElabYFraccion", valor);
			}

		}
	}
}
