
namespace gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra
{
    public class ConceptoFacturadoDto : Dto
    {
		public string concepto { get; set; } = string.Empty;
		public int cantidad { get; set; } = 1;
		public string iva_situacion { get; set; } = string.Empty;
		public decimal iva_alicuota { get; set; } = 0.00M;
		public decimal subtotal { get; set; } = 0.00M;
		public decimal iva { get; set; } = 0.00M;
		public decimal total { get; set; } = 0.00M;
	}
}
