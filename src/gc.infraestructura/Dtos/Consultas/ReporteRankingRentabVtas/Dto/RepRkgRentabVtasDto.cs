
namespace gc.infraestructura.Dtos
{
	public class RepRkgRentabVtasDto : Dto
	{
		public string p_id { get; set; } = string.Empty;
		public string p_id_barrado { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public string rubg_id { get; set; } = string.Empty;
		public string rubg_desc { get; set; } = string.Empty;
		public string sec_id { get; set; } = string.Empty;
		public string sec_desc { get; set; } = string.Empty;
		public int vtas_cantidad { get; set; }
		public decimal vtas_cantidad_porc { get; set; }
		public decimal vtas_facturacion { get; set; }
		public decimal vtas_facturacion_porc { get; set; }
		public decimal vtas_neto { get; set; }
		public decimal vtas_costo { get; set; }
		public decimal vtas_rentabilidad { get; set; }
		public decimal vtas_rentabilidad_porc { get; set; }
		public decimal vtas_renta_costo_porc { get; set; }
	}
}
