using System;

namespace gc.infraestructura.Dtos.Almacen
{
    public partial class CuentaDto:Dto
    {
        public CuentaDto()
        {
          
        }
		public string Ib_id { get; set; } = string.Empty;
        public string Cta_Id { get; set; } = string.Empty;
        public string Cta_Denominacion { get; set; } = string.Empty;
        public string Tdoc_Id { get; set; } = string.Empty;
        public string Cta_Documento { get; set; } = string.Empty;
        public string Cta_Domicilio { get; set; } = string.Empty;
        public string Cta_Localidad { get; set; } = string.Empty;
        public string Cta_Cpostal { get; set; } = string.Empty;
        public char? Prov_Id { get; set; }
        public string Dep_Id { get; set; } = string.Empty;
        public string Cta_Te { get; set; } = string.Empty;
        public string Cta_Celu { get; set; } = string.Empty;
        public string Cta_Email { get; set; } = string.Empty;
        public string Cta_Www { get; set; } = string.Empty;
        public string Afip_Id { get; set; } = string.Empty;
        public string Nj_Id { get; set; } = string.Empty;
        public string Cta_Ib_Nro { get; set; } = string.Empty;
        public char? Cta_Ib_Regimen { get; set; }
        public char? Tcb_Id { get; set; }
        public string Cta_Bco_Cuenta_Nro { get; set; } = string.Empty;
        public string Cta_Bco_Cuenta_Cbu { get; set; } = string.Empty;
        public DateTime? Cta_Alta { get; set; }
        public string Cta_Obs { get; set; } = string.Empty;
        public DateTime? Cta_Cuit_Vto { get; set; }
        public char Cta_Emp { get; set; }
        public string Cta_Emp_Legajo { get; set; } = string.Empty;
        public string Cta_Emp_Ctaf { get; set; } = string.Empty;
        public DateTime? Cta_Actu_Fecha { get; set; }
        public char? Cta_Actu { get; set; }
        public char? Tipo { get; set;}
        public char? Habilitada { get; set; }
		public string Tdoc_Desc { get; set; } = string.Empty;

        /// <summary>
        /// Es un dato auxiliar para ser utilizado en los reportes. 
        /// </summary>
        public decimal Monto { get; set; }
        public string MontoEtiqueta { get; set; } = string.Empty;
    }

    public partial class  CuentaABMDto : CuentaDto
	{
		public string Prov_Nombre { get; set; } = string.Empty;
		public string Dep_Nombre { get; set; } = string.Empty;
		public string Afip_Desc { get; set; } = string.Empty;
		public string Nj_Desc { get; set; } = string.Empty;
		public string Ib_Id { get; set; } = string.Empty;
		public string Ib_Desc { get; set; } = string.Empty;
		public string Tcb_Desc { get; set; } = string.Empty;
		public decimal Ctac_Tope_Credito { get; set; } = 0.000M;
		public decimal? Ctac_Tope_Credito_Dia { get; set; } = 0.000M;
		public decimal? Ctac_Tope_Credito_Dia_Ult { get; set; } = 0.000M;
		public decimal Ctac_Dto_Operacion { get; set; } = 0.000M;
		public decimal? Ctac_Dto_Operacion_Dia { get; set; } = 0.000M;
		public decimal? Ctac_Dto_Operacion_Dia_Ult { get; set; } = 0.000M;
        public string Piva_Cert { get; set; } = string.Empty;
        public DateTime? Piva_Cert_Vto { get; set; }
		public string Pib_Cert { get; set; } = string.Empty;
		public DateTime? Pib_Cert_Vto { get; set; }
        public int? Ctac_Ptos_Vtas { get; set; }
        public DateTime? Ctac_Negocio_Inicio { get; set; }
        public string Ctn_Id { get; set; } = string.Empty;
		public string Ctn_Desc { get; set; } = string.Empty;
		public string Ctc_Id { get; set; } = string.Empty;
		public string Ctc_Desc { get; set; } = string.Empty;
		public string Ve_Id { get; set; } = string.Empty;
		public string Ve_Nombre { get; set; } = string.Empty;
		public char? Ve_Visita { get; set; }
		public string Zn_Id { get; set; } = string.Empty;
		public string Zn_Desc { get; set; } = string.Empty;
		public string Rp_Id { get; set; } = string.Empty;
		public string Rp_Nombre { get; set; } = string.Empty;
		public string Lp_Id { get; set; } = string.Empty;
		public string Lp_Desc { get; set; } = string.Empty;
        public char Ctac_Habilitada { get; set; }
        public bool Cta_Activa
        {
            get 
            {
                if (char.IsWhiteSpace(Ctac_Habilitada) || string.IsNullOrWhiteSpace(char.ToString(Ctac_Habilitada)))
                    return false;
                return Ctac_Habilitada == 'S';
            }
            set { cta_Activa = value; }
        }
        private bool cta_Activa;

		public bool Piva_Cert_Activa
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Piva_Cert))
					return false;
				return Piva_Cert.Trim() == "S";
			}
			set { piva_Cert_Activa = value; }
		}
		private bool piva_Cert_Activa;

		public bool Pib_Cert_Activa
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Pib_Cert))
					return false;
				return Pib_Cert.Trim() == "S";
			}
			set { pib_Cert_Activa = value; }
		}
		private bool pib_Cert_Activa;

		public bool Cta_Emp_Activa
		{
			get
			{
				if (char.IsWhiteSpace(Cta_Emp) || string.IsNullOrWhiteSpace(char.ToString(Cta_Emp)))
					return false;
				return Cta_Emp == 'S';
			}
			set { cta_Emp_Activa = value; }
		}
		private bool cta_Emp_Activa;

	}

	public class CuentaDatoDto : Dto
	{
        public string Cta_Id { get; set; } = string.Empty;
        public string Cta_Nombre { get; set; } = string.Empty;   
        public string Cta_Te { get; set; } = string.Empty;
        public string Cta_Celu { get; set; } = string.Empty;
        public string Cta_Email { get; set; } = string.Empty;
        public char Tc_Id { get; set; }        
    }
}
