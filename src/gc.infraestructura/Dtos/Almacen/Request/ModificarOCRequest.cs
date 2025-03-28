
namespace gc.infraestructura.Dtos.Almacen.Request
{
    public class ModificarOCRequest
    {
		public string oc_compte { get; set; }
		public string usu_id { get; set; }
		public string adm_id { get; set; }
		public string adm_id_entrega { get; set; }
		public AccionesSobreLasOC accion { get; set; }
	}
}
