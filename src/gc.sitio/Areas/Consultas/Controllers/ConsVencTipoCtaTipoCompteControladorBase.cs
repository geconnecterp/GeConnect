using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte;
using gc.infraestructura.Dtos.Financieros;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Consultas.Controllers
{
	public class ConsVencTipoCtaTipoCompteControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;

		public ConsVencTipoCtaTipoCompteControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;

		}

		public List<VencimientoListaDto> ListaVencimientos
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaVencimientos");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<VencimientoListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaVencimientos", json);
			}
		}

		public MetadataGrid MetadataVencimientos
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataVencimientos");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt);
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataVencimientos", valor);
			}

		}
	}
}
