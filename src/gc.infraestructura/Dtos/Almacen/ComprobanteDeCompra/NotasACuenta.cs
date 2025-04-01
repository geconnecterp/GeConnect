
namespace gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra
{
    public class NotasACuenta : Dto
    {
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc_ori { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string tco_id_rp { get; set; } = string.Empty;
		public string tco_desc_rp { get; set; } = string.Empty;
		public string cm_compte_rp { get; set; } = string.Empty;
		public string dia_movi_rp { get; set; } = string.Empty;
		public decimal cm_gravado { get; set; } = 0.00M;
		public decimal cm_no_gravado { get; set; } = 0.00M;
		public decimal cm_exento { get; set; } = 0.00M;
		public decimal cm_ii { get; set; } = 0.00M;
		public decimal cm_otro_ng { get; set; } = 0.00M;
		public decimal cm_iva { get; set; } = 0.00M;
		public decimal cm_percepciones { get; set; } = 0.00M;
		public decimal cm_total { get; set; } = 0.00M;
		public string tco_id_nc { get; set; } = string.Empty;
		public string cm_compte_nc { get; set; } = string.Empty;
		public string dia_movi_nc { get; set; } = string.Empty;
	}
}
