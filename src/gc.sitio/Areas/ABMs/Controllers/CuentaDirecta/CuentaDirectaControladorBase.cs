using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers.CuentaDirecta
{
	[Area("ABMs")]
	public class CuentaDirectaControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ILogger _logger;

		public CuentaDirectaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_logger = logger;
		}

		#region ABM
		/// <summary>
		/// Permite verificar que pagina se esta observando.
		/// </summary>
		public int PaginaBanco
		{
			get
			{
				var txt = _context.HttpContext.Session.GetString("PaginaBanco");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return 0;
				}
				return txt.ToInt();
			}
			set
			{
				var valor = value.ToString();
				_context.HttpContext.Session.SetString("PaginaBanco", valor);
			}
		}

		public string DirSortCuentaDirecta
		{
			get
			{
				var txt = _context.HttpContext.Session.GetString("DirSortCuentaDirecta");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return "asc";
				}
				return txt;
			}
			set
			{
				var valor = value.ToString();
				_context.HttpContext.Session.SetString("DirSortCuentaDirecta", valor);
			}
		}

		public List<ABMCuentaDirectaSearchDto> CuentaDirectaBuscadas
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("CuentaDirectaBuscadas");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ABMCuentaDirectaSearchDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("CuentaDirectaBuscadas", json);
			}
		}

		public MetadataGrid MetadataCuentaDirecta
		{
			get
			{
				var txt = _context.HttpContext.Session.GetString("MetadataCuentaDirecta");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt); ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("MetadataCuentaDirecta", valor);
			}

		}

		#endregion

		protected enum Abm
		{
			A = 1, //Alta
			B = 2, //Baja
			M = 3, //Modificacion
			S = 4, //Submit
			C = 5  //Cancel
		}
	}
}
