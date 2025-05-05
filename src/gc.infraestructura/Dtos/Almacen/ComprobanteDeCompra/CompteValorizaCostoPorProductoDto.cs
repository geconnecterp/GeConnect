
namespace gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra
{
	public class CompteValorizaCostoPorProductoDto : Dto
	{		
		public decimal ocd_plista { get; set; } = 0.00M;
		public decimal ocd_dto1 { get; set; } = 0.00M;
		public decimal ocd_dto2 { get; set; } = 0.00M;
		public decimal ocd_dto3 { get; set; } = 0.00M;
		public decimal ocd_dto4 { get; set; } = 0.00M;
		public decimal ocd_dto_pa { get; set; } = 0.00M;
		public string ocd_boni { get; set; } = string.Empty;
		public decimal ocd_pcosto { get; set; } = 0.00M;
	}
}
