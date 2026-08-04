using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Consultas.Controllers.ReporteEvalDeNivelDeServicio
{
	public class ReporteEvalDeNivelDeServicioControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ReporteEvalDeNivelDeServicioControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<ReporteEvalDeNivelDeServicioDto> ListaProductoEvalDeNivelDeServicio
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaProductoEvalDeNivelDeServicio");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ReporteEvalDeNivelDeServicioDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaProductoEvalDeNivelDeServicio", json);
			}
		}
	}
}
