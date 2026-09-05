using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Tr
{
	public class PedidoInternoRequest : RequestBase
	{
		public DateTime fecha_d { get; set; }
		public DateTime fecha_h { get; set; }
		public bool adm { get; set; }
		public string adm_list { get; set; } = string.Empty;
		public bool estado { get; set; }
		public string estado_list { get; set; } = string.Empty;
		public int Registros { get; set; }
		public int Pagina { get; set; }
	}
}
