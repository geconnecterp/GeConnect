using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Financieros;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Consultas.Controllers
{
	public class ReporteFinancieroControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ReporteFinancieroControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<GastoProyListaDto> ListaProyeccionDeEgresos
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaProyeccionDeEgresos");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<GastoProyListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaProyeccionDeEgresos", json);
			}
		}
	}
}
