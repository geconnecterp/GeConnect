using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.CuentaComercial
{
    public class CuentaGastoDto : Dto
    {
		public string Ctag_Id { get; set; } = string.Empty;
		public string Ctag_Denominacion { get; set; } = string.Empty;
		public string Tcg_Id { get; set; } = string.Empty;
		public string Tcg_Desc { get; set; } = string.Empty;
		public bool Ctag_Ingreso { get; set; }
		public string Ctag_Valores_Anombre { get; set; } = string.Empty;
		public char Ctag_Activo { get; set; }
		public string Ccb_Id { get; set; } = string.Empty;
		public string Ccb_Desc { get; set; } = string.Empty;

		public bool Ctag_Activa
		{
			get
			{
				if (char.IsWhiteSpace(Ctag_Activo) || string.IsNullOrWhiteSpace(char.ToString(Ctag_Activo)))
					return false;
				return Ctag_Activo == 'S';
			}
			set { ctag_Activa = value; }
		}
		private bool ctag_Activa;
	}
}
