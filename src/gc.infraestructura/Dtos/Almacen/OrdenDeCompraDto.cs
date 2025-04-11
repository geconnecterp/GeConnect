
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.Almacen
{
	public class OrdenDeCompraConsultaDto : OrdenDeCompraListDto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public decimal oc_total { get; set; } = 0.00M;
	}

	public class OrdenDeCompraListDto : Dto
	{
		public string oc_compte { get; set; } = string.Empty;
		public string oc_fecha { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public char? oce_id { get; set; }
		public string? oce_desc { get; set; }
	}

	public class OrdenDeCompraCargaDto : Dto
	{
		public string Lim_Mensual { get; set; } = string.Empty;
		public string OC_Emitidas { get; set; } = string.Empty;
		public string Tope_Emision { get; set; } = string.Empty;
		public SelectList ComboSucursales { get; set; }
	}

	public class OrdenDeCompraDto : Dto
	{
		public string Oc_Compte { get; set; } = string.Empty;
		public DateTime Oc_Fecha { get; set; }
		public int Oc_Entrega_Dias { get; set; }
		public DateTime Oc_Entrega_Fecha { get; set; }
		public char Oc_Pago_Ant { get; set; }
		public DateTime? Oc_Pago_Ant_Vto { get; set; }
		public char? Oce_Id { get; set; }
		public string Cta_Id { get; set; } = string.Empty;
		public string Usu_Id { get; set; } = string.Empty;
		public string Adm_Id { get; set; } = string.Empty;
		public string Depo_Id { get; set; } = string.Empty;
		public decimal Oc_Pcosto { get; set; } = 0.00M;
		public decimal Oc_Dto_Porc { get; set; } = 0.00M;
		public decimal Oc_Dto { get; set; } = 0.00M;
		public decimal Oc_Flete_Porc { get; set; } = 0.00M;
		public decimal Oc_Flete_Importe { get; set; } = 0.00M;
		public decimal Oc_Flete_Iva_Ali { get; set; } = 0.00M;
		public decimal Oc_Flete_Iva { get; set; } = 0.00M;
		public decimal Oc_Gravado { get; set; } = 0.00M;
		public decimal Oc_No_Gravado { get; set; } = 0.00M;
		public decimal Oc_Exento { get; set; } = 0.00M;
		public decimal Oc_In { get; set; } = 0.00M;
		public decimal Oc_Iva { get; set; } = 0.00M;
		public decimal Oc_Percepciones { get; set; } = 0.00M;
		public string? Oc_Observaciones { get; set; }
		public string Oce_Desc { get; set; } = string.Empty;
		public string Cta_Denominacion { get; set; } = string.Empty;
		public string Adm_Nombre { get; set; } = string.Empty;
		public string Usu_Apellidoynombre { get; set; } = string.Empty;
	}

	public enum AccionesSobreLasOC
	{
		ACTIVAR,
		CERRAR,
		ANULAR,
		LEVANTAR,
		MODIFICAR_ADM
	}

	public class ObtenerAccionesSobreLasOC
	{
		public static AccionesSobreLasOC ObtenerActivar()
		{
			return AccionesSobreLasOC.ACTIVAR;
		}
		public static string ObtenerCerrar()
		{
			return AccionesSobreLasOC.CERRAR.ToString();
		}
		public static string ObtenerAnular()
		{
			return AccionesSobreLasOC.ANULAR.ToString();
		}
		public static string ObtenerLevantar()
		{
			return AccionesSobreLasOC.LEVANTAR.ToString();
		}
		public static string ObtenerModificarAdm()
		{
			return AccionesSobreLasOC.MODIFICAR_ADM.ToString();
		}
	}
}
