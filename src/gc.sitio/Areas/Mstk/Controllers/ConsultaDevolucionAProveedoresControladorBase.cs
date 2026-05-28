using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Mstk.Controllers
{
	public class ConsultaDevolucionAProveedoresControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ConsultaDevolucionAProveedoresControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<DevolucionProveedoresListaDto> ListaDevoluciones
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaDevoluciones");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<DevolucionProveedoresListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaDevoluciones", json);
			}
		}

		public MetadataGrid MetadataListaDevoluciones
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataListaDevoluciones");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt);
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataListaDevoluciones", valor);
			}

		}
	}
}
