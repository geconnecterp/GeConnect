
namespace gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra
{
    public class RprAsociadosDto : Dto
    {
		public string rp_compte { get; set; } = string.Empty;
		public DateTime rp_fecha { get; set; }
		public string tco_id_rp { get; set; } = string.Empty;
		public string cm_compte_rp { get; set; } = string.Empty;
		public DateTime cm_fecha_rp { get; set; }
		public decimal cm_importe_rp { get; set; } = 0.00M;
		public string cta_id { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public char rpe_id { get; set; }
		public string rpe_desc { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
	}
}
