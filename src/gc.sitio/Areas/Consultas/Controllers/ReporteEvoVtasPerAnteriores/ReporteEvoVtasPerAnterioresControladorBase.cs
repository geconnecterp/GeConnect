using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Consultas.Controllers.ReporteEvoVtasPerAnteriores
{
	public class ReporteEvoVtasPerAnterioresControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ReporteEvoVtasPerAnterioresControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}
		public List<ReporteEvoVtasPerAnterioresDto> ListaProductoEvo
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaProductoEvo");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ReporteEvoVtasPerAnterioresDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaProductoEvo", json);
			}
		}
	}
}
