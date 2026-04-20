using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Ventas;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Ventas.Controllers
{
	public class VentasCajasCorreccionValoresControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public VentasCajasCorreccionValoresControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
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

		public List<VtasPVCtlRendDetalleDto> VtasPVCtlRendDetalleLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("VtasPVCtlRendDetalleLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<VtasPVCtlRendDetalleDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("VtasPVCtlRendDetalleLista", json);
			}
		}
		
	}
}
