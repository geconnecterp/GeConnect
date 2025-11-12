using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Financieros;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	public class LiqDeEmpConsultaYAnulacionControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public LiqDeEmpConsultaYAnulacionControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<LiqDeEmpleadoListaDto> ListaLiqDeEmp
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaLiqDeEmp");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<LiqDeEmpleadoListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaLiqDeEmp", json);
			}
		}

		public MetadataGrid MetadataLiqDeEmp
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataLiqDeEmp");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt);
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataLiqDeEmp", valor);
			}

		}

		public List<LiqEmpleadoFileBcoDto> ListaLiqDeEmpFileBco
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaLiqDeEmpFileBco");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<LiqEmpleadoFileBcoDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaLiqDeEmpFileBco", json);
			}
		}
	}
}
