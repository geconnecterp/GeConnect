
namespace gc.infraestructura.Dtos.Almacen
{
    public class OrdenDeCompraConceptoDto : Dto
    {
		public string id { get; set; } = string.Empty;
		public int Orden { get; set; }
		public string Concepto { get; set; } = string.Empty;
		public decimal Importe { get; set; } = 0.000M;
	}
}
