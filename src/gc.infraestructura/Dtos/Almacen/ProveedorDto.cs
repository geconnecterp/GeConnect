using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Dtos.Almacen
{
	public partial class ProveedorDto : Dto
	{
		public ProveedorDto()
		{
			Cta_Id = string.Empty;
			Ctap_Ean = string.Empty;
			Ctap_Id_Externo = string.Empty;
			Ctap_Viajante = string.Empty;
			Ctap_Viajante_Ce = string.Empty;
			Ctap_Viajante_Email = string.Empty;
			Ctap_Valores_A_Nombre = string.Empty;
			Rgan_Id = string.Empty;
			Ope_Iva = string.Empty;
			Ctag_Id = string.Empty;
			Ctap_Obs_Op = string.Empty;
			Ctap_Obs_Precios = string.Empty;
			Id_Old = string.Empty;
		}
		public string Cta_Id { get; set; }
		public char? Tp_Id { get; set; }
		public char? Fp_Id { get; set; }
		public short? Fp_Dias { get; set; }
		public string Ctap_Ean { get; set; }
		public string Ctap_Id_Externo { get; set; }
		public string Ctap_Viajante { get; set; }
		public string Ctap_Viajante_Ce { get; set; }
		public string Ctap_Viajante_Email { get; set; }
		public string Ctap_Valores_A_Nombre { get; set; }
		public short? Ctap_Rp_Plazo_Compra { get; set; }
		public short? Ctap_Rp_Plazo_Entrega { get; set; }
		public char Ctap_Rgan { get; set; }
		public string Rgan_Id { get; set; }
		public char Rgan_Cert { get; set; }
		public DateTime? Rgan_Cert_Vto { get; set; }
		public decimal Rgan_Porc { get; set; }
		public char Ctap_Rib { get; set; }
		public char? Rib_Id { get; set; }
		public char Rib_Cert { get; set; }
		public DateTime? Rib_Cert_Vto { get; set; }
		public decimal Rib_Porc { get; set; }
		public char? Ctap_Ret_Iva { get; set; }
		public decimal? Ctap_Ret_Iva_Porc { get; set; }
		public char Ctap_Per_Iva { get; set; }
		public decimal Ctap_Per_Iva_Ali { get; set; }
		public char Ctap_Per_Ib { get; set; }
		public decimal Ctap_Per_Ib_Ali { get; set; }
		public char? Ctap_Pago_Susp { get; set; }
		public char Ctap_Devolucion { get; set; }
		public char? Ctap_Devolucion_Flete { get; set; }
		public char? Ctap_Acuenta_Dev { get; set; }
		public decimal? Ctap_D1 { get; set; }
		public decimal? Ctap_D2 { get; set; }
		public decimal? Ctap_D3 { get; set; }
		public decimal? Ctap_D4 { get; set; }
		public decimal? Ctap_D5 { get; set; }
		public decimal? Ctap_D6 { get; set; }
		public string Ope_Iva { get; set; }
		public string Ctag_Id { get; set; }
		public string Ctap_Obs_Op { get; set; }
		public string Ctap_Obs_Precios { get; set; }
		public char Ctap_Habilitada { get; set; }
		public string Id_Old { get; set; }

	}

	public partial class ProveedorABMDto : ProveedorDto
	{
		public string Cta_Www { get; set; } = string.Empty;
		public string Cta_Localidad { get; set; } = string.Empty;
		public string Cta_Domicilio { get; set; } = string.Empty;
		public string Cta_Denominacion { get; set; } = string.Empty;
		public string Tdoc_Id { get; set; } = string.Empty;
		public string Tdoc_desc { get; set; } = string.Empty;
		public string Cta_Documento { get; set; } = string.Empty;
		public string Prov_Id { get; set; } = string.Empty;
		public string Prov_Nombre { get; set; } = string.Empty;
		public string Cta_Cpostal { get; set; } = string.Empty;
		public string Dep_Id { get; set; } = string.Empty;
		public string Dep_Nombre { get; set; } = string.Empty;
		public string Afip_Id { get; set; } = string.Empty;
		public string Afip_Desc { get; set; } = string.Empty;
		public string Nj_Id { get; set; } = string.Empty;
		public string Nj_Desc { get; set; } = string.Empty;
		public string Cta_Ib_Nro { get; set; } = string.Empty;
		public string Ib_Id { get; set; } = string.Empty;
		public string Ib_Desc { get; set; } = string.Empty;
		public string Cta_Emp { get; set; } = string.Empty;
		public DateTime? Cta_Actu_Fecha { get; set; }
		public string Cta_Actu { get; set; } = string.Empty;
		public string Ope_Iva_Descripcion { get; set; } = string.Empty;
		public bool Cta_Activa
		{
			get
			{
				if (char.IsWhiteSpace(Ctap_Habilitada) || string.IsNullOrWhiteSpace(char.ToString(Ctap_Habilitada)))
					return false;
				return Ctap_Habilitada == 'S';
			}
			set { cta_Activa = value; }
		}
		private bool cta_Activa;

		public bool Ctap_Devolucion_bool
		{
			get
			{
				if (char.IsWhiteSpace(Ctap_Devolucion) || string.IsNullOrWhiteSpace(char.ToString(Ctap_Devolucion)))
					return false;
				return Ctap_Devolucion == 'S';
			}
			set { ctap_Devolucion_bool = value; }
		}
		private bool ctap_Devolucion_bool;

		public bool Ctap_Devolucion_Flete_bool
		{
			get
			{
				if (Ctap_Devolucion_Flete == null)
					return false;
				if (char.IsWhiteSpace(Ctap_Devolucion_Flete.Value) || string.IsNullOrWhiteSpace(char.ToString(Ctap_Devolucion_Flete.Value)))
					return false;
				return Ctap_Devolucion_Flete == 'S';
			}
			set { ctap_Devolucion_Flete_bool = value; }
		}
		private bool ctap_Devolucion_Flete_bool;

		public bool Ctap_Acuenta_Dev_bool
		{
			get
			{
				if (Ctap_Acuenta_Dev == null)
					return false;
				if (char.IsWhiteSpace(Ctap_Acuenta_Dev.Value) || string.IsNullOrWhiteSpace(char.ToString(Ctap_Acuenta_Dev.Value)))
					return false;
				return Ctap_Acuenta_Dev == 'S';
			}
			set { ctap_Acuenta_Dev_bool = value; }
		}
		private bool ctap_Acuenta_Dev_bool;

		public bool Ctap_Pago_Susp_bool
		{
			get
			{
				if (Ctap_Pago_Susp == null)
					return false;
				if (char.IsWhiteSpace(Ctap_Pago_Susp.Value) || string.IsNullOrWhiteSpace(char.ToString(Ctap_Pago_Susp.Value)))
					return false;
				return Ctap_Pago_Susp == 'S';
			}
			set { ctap_Pago_Susp_bool = value; }
		}
		private bool ctap_Pago_Susp_bool;

		public bool Ctap_Rgan_bool
		{
			get
			{
				if (char.IsWhiteSpace(Ctap_Rgan) || string.IsNullOrWhiteSpace(char.ToString(Ctap_Rgan)))
					return false;
				return Ctap_Rgan == 'S';
			}
			set { ctap_Rgan_bool = value; }
		}
		private bool ctap_Rgan_bool;

		public bool Ctap_Rgan_Cert_bool
		{
			get
			{
				if (char.IsWhiteSpace(Rgan_Cert) || string.IsNullOrWhiteSpace(char.ToString(Rgan_Cert)))
					return false;
				return Rgan_Cert == 'S';
			}
			set { ctap_Rgan_Cert_bool = value; }
		}
		private bool ctap_Rgan_Cert_bool;

	}
}
