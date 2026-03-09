
namespace gc.infraestructura.Dtos.Productos.Pedidos
{
	public class ConfirmarPedidoDto : Dto
	{
		public char Abm { get; set; } // A: alta, B: baja, M: modificacion
		public PedidoDto Datos { get; set; } = new();
		public List<PedidoElementoDto> Productos { get; set; } = [];
	}
}
