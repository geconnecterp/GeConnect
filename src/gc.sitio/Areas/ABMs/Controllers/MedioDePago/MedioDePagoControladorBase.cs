using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers.MedioDePago
{
	[Area("ABMs")]
	public class MedioDePagoControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ILogger _logger;
		public MedioDePagoControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
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
				var txt = _context.HttpContext.Session.GetString("PaginaProd");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return 0;
				}
				return txt.ToInt();
			}
			set
			{
				var valor = value.ToString();
				_context.HttpContext.Session.SetString("PaginaProd", valor);
			}
		}

		public string DirSortProd
		{
			get
			{
				var txt = _context.HttpContext.Session.GetString("DirSortProd");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return "asc";
				}
				return txt;
			}
			set
			{
				var valor = value.ToString();
				_context.HttpContext.Session.SetString("DirSortProd", valor);
			}
		}

		public List<ABMMedioDePagoSearchDto> MediosDePagoBuscados
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("MediosDePagoBuscados");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ABMMedioDePagoSearchDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("MediosDePagoBuscados", json);
			}
		}

		public MetadataGrid MetadataMedioDePago
		{
			get
			{
				var txt = _context.HttpContext.Session.GetString("MetadataMedioDePago");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt); ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("MetadataMedioDePago", valor);
			}

		}
		#endregion

		#region Enum's
		/// <summary>
		/// Objetco involucrado en la operación de ABM
		/// </summary>
		protected enum AbmObject
		{
			medios_de_pago = 1,
			opciones_cuota = 2,
			cuenta_financiera_contable = 3,
			pos = 4
		}

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
