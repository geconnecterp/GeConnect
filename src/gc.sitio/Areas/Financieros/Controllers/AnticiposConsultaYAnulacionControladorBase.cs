using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Financieros;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	public class AnticiposConsultaYAnulacionControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public AnticiposConsultaYAnulacionControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<AnticipoFinanEmpListaDto> ListaAnticipoFinanEmp
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaAnticipoFinanEmp");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<AnticipoFinanEmpListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaAnticipoFinanEmp", json);
			}
		}

		public MetadataGrid MetadataAnticipoFinanEmp
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataAnticipoFinanEmp");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt);
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataAnticipoFinanEmp", valor);
			}

		}

		public List<FinancieroUsuarioDto> FinancieroUsuariosLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("FinancieroUsuariosLista") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<FinancieroUsuarioDto>();
				}
				return JsonConvert.DeserializeObject<List<FinancieroUsuarioDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("FinancieroUsuariosLista", json);
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
	}
}
