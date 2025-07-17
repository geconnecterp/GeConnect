using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.sitio.Areas.Compras.Models.OrdenDePagoConsulta;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers
{
	public class OrdenDePagoConsultaControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public OrdenDePagoConsultaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
		}

		public List<OrdenDePagoConsultaDto> ListaOrdenDePagoConsulta
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaOrdenDePagoConsulta");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<OrdenDePagoConsultaDto>();
				}
				return JsonConvert.DeserializeObject<List<OrdenDePagoConsultaDto>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaOrdenDePagoConsulta", valor);
			}

		}

		public List<OPUserDto> ListaOPUsuarios
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaOPUsuarios");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<OPUserDto>();
				}
				return JsonConvert.DeserializeObject<List<OPUserDto>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaOPUsuarios", valor);
			}

		}

		public List<TipoCertificadoModel> ListaTipoCertificado
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaTipoCertificado");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<TipoCertificadoModel>();
				}
				return JsonConvert.DeserializeObject<List<TipoCertificadoModel>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaTipoCertificado", valor);
			}

		}

		public enum TipoCertificado
		{
			IngresosBrutos = 1,
			Ganancias=2,
			IVA=3,
		}
	}
}
