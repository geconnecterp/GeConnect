
namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class NCPIConfirmarCambiosPedidoAutoRequest 
	{
		public string tipo { get; set; } = string.Empty;
		public string json_p { get; set; } = string.Empty;
		public int dias_prevision { get; set; }
		public DateTime vta_ana_desde { get; set; }
		public DateTime vta_ana_hasta { get; set; }
		public bool limite_max { get; set; }
		public bool limite_min { get; set; }
		public bool ultimo_ped { get; set; }
		public bool? excluir_pend { get; set; }
		public bool es_pedido_interno { get; set; }
		public List<string> adm_list { get; set; } = [];
		public List<string> depo_list { get; set; } = [];
	}
}
