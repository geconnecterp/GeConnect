using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers
{
	[Area("ABMs")]
	public class SectorControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public SectorControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
		}

		#region ABM
		/// <summary>
		/// Permite verificar que pagina se esta observando.
		/// </summary>
		public int PaginaSector
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("PaginaSector");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return 0;
				}
				return txt.ToInt();
			}
			set
			{
				var valor = value.ToString();
				_context.HttpContext?.Session.SetString("PaginaSector", valor);
			}
		}

		public string DirSortSector
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("DirSortSector");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return "asc";
				}
				return txt;
			}
			set
			{
				var valor = value.ToString();
				_context.HttpContext?.Session.SetString("DirSortSector", valor);
			}
		}
		public List<ABMSectorSearchDto> SectoresBuscados
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("SectoresBuscados");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ABMSectorSearchDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("SectoresBuscados", json);
			}
		}

		/// <summary>
		/// Lista de subsectores, pertenecientes al sector seleccionado
		/// </summary>
		public List<SubSectorDto> SubSectorLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("SubSectores");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<SubSectorDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("SubSectores", json);
			}
		}

		public MetadataGrid MetadataSector
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataSector");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt)??new() ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataSector", valor);
			}

		}
		#endregion
	}
}
