using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using gc.sitio.Controllers;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers.MedioDePago
{
	[Area("ABMs")]
	public class MedioDePagoControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public MedioDePagoControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
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

		public List<ABMMedioDePagoSearchDto> MediosDePagoBuscados
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("MediosDePagoBuscados");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ABMMedioDePagoSearchDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MediosDePagoBuscados", json);
			}
		}

		public MetadataGrid MetadataMedioDePago
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataMedioDePago");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt)??new() ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataMedioDePago", valor);
			}

		}

		public Pos PosSeleccionado
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("PosSeleccionado");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new Pos();
				}
				return JsonConvert.DeserializeObject<Pos>(txt)??new() ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("PosSeleccionado", valor);
			}

		}

		public MedioDePagoABMDto MedioDePagoSelected
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MedioDePagoSelected");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MedioDePagoABMDto();
				}
				return JsonConvert.DeserializeObject<MedioDePagoABMDto>(txt)??new() ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MedioDePagoSelected", valor);
			}

		}

		public string TCSelected
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("TCSelected");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return string.Empty;
				}
				return JsonConvert.DeserializeObject<string>(txt)??string.Empty ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("TCSelected", valor);
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

		public class Pos()
		{
			public string Ins_Id { get; set; } = string.Empty;
			public string? Ins_Id_Pos { get; set; }
			public string? Ins_Id_Pos_Ctls { get; set; }
		}
		#endregion
	}
}
