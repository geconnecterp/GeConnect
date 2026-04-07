using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Tr
{
	public class ConfirmarPedidoInternoRequest : RequestBase
	{
		public string adm_id_entrega { get; set; } = string.Empty;
		public string json { get; set; } = string.Empty;
	}
}
