using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Almacen.RemitoExterno;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Mstk.Controllers.ConsultaDeRemitoExterno
{
	public class ConsultaDeRemitoExternoControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ConsultaDeRemitoExternoControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<RemitoExternoListaDto> ListaRemitosExternos
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaRemitosExternos");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<RemitoExternoListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaRemitosExternos", json);
			}
		}

		public MetadataGrid MetadataListaRemitosExternos
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataListaRemitosExternos");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt);
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataListaRemitosExternos", valor);
			}

		}
	}
}
