using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Mstk;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Consultas.Controllers.ReporteRankingRentabVtas
{
	public class ReporteRankingRentabVtasControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ReporteRankingRentabVtasControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}
		public List<RepRkgRentabVtasDto> ListaProductoRnk
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaProductoRnk");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<RepRkgRentabVtasDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaProductoRnk", json);
			}
		}
	}
}
