using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	public class BancosControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public BancosControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<FinancieroChequeDepositadoDto> ListaChequesAgrupados
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaChequesAgrupados");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<FinancieroChequeDepositadoDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaChequesAgrupados", json);
			}
		}

		public List<FinancieroChequeDepositadoDto> ListaChequesDetalles
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaChequesDetalles");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<FinancieroChequeDepositadoDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaChequesDetalles", json);
			}
		}
	}
}
