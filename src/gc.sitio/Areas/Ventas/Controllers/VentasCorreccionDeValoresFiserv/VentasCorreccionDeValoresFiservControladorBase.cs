using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Ventas;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Ventas.Controllers.VentasCorreccionDeValoresFiserv
{
	public class VentasCorreccionDeValoresFiservControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public VentasCorreccionDeValoresFiservControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<CuentaDto> ClientesLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ClientesLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<CuentaDto>();
				}
				return JsonConvert.DeserializeObject<List<CuentaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ClientesLista", json);
			}
		}

		public List<VtasPVCtlProcesoDto> VtasPVCtlProcesoLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("VtasPVCtlProcesoLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<VtasPVCtlProcesoDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("VtasPVCtlProcesoLista", json);
			}
		}
	}
}
