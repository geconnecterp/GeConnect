using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
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

		public List<VtasPVCtlCierresDto> VtasPVCtlCierresLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("VtasPVCtlCierresLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<VtasPVCtlCierresDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("VtasPVCtlCierresLista", json);
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

		public List<ABMChequeListaDto> ChequesLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ChequesLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<ABMChequeListaDto>();
				}
				return JsonConvert.DeserializeObject<List<ABMChequeListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ChequesLista", json);
			}
		}
	}
}
