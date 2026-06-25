using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Consultas.Controllers
{
	public class MovYCarteraDeCuentaFinanControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public MovYCarteraDeCuentaFinanControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}
		public List<FinancieroCuentaListaDto> ListaFinancieroCuenta
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaFinancieroCuenta");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<FinancieroCuentaListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaFinancieroCuenta", json);
			}
		}

		public FinancieroCuentaListaDto FinancieroCuentaSeleccionada
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("FinancieroCuentaSeleccionada");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return null;
				}
				return JsonConvert.DeserializeObject<FinancieroCuentaListaDto>(json) ?? null;
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("FinancieroCuentaSeleccionada", json);
			}
		}
	}
}
