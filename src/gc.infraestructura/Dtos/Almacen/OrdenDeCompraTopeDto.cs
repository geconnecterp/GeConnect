
namespace gc.infraestructura.Dtos.Almacen
{
    public class OrdenDeCompraTopeDto : Dto
    {
		public decimal oc_limite_semanal { get; set; } = 0.00M;
		public decimal oc_emitidas { get; set; } = 0.00M;
		public decimal oc_tope { get; set; } = 0.00M;
	}
}
