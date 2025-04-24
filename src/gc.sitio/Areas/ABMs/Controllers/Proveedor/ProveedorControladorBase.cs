using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers
{
	[Area("ABMs")]
	public class ProveedorControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ILogger _logger;
		public ProveedorControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_logger = logger;
		}

		#region ABM
		/// <summary>
		/// Permite verificar que pagina se esta observando.
		/// </summary>
		public int PaginaProd
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("PaginaProd");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return 0;
				}
				return txt.ToInt();
			}
			set
			{
				var valor = value.ToString();
				_context.HttpContext?.Session.SetString("PaginaProd", valor);
			}
		}

		public string DirSortProd
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("DirSortProd");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return "asc";
				}
				return txt;
			}
			set
			{
				var valor = value.ToString();
				_context.HttpContext?.Session.SetString("DirSortProd", valor);
			}
		}

		public List<ABMProveedorSearchDto> ProveedoresBuscados
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ProveedoresBuscados");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ABMProveedorSearchDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ProveedoresBuscados", json);
			}
		}

		public MetadataGrid MetadataProveedor
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataProveedor");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt); ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataProveedor", valor);
			}

		}
		#endregion

		#region Enum's
		protected enum Abm
		{
			A = 1, //Alta
			B = 2, //Baja
			M = 3, //Modificacion
			S = 4, //Submit
			C = 5  //Cancel
		}
		#endregion
	}
}
