using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;

namespace gc.sitio.Areas.Distribuidora.Models.OrdenDeReparto
{
	public class OrdenDeRepartoPonerEnCursoModel
	{
		public OrdenDeRepartoDto OrdenDeReparto { get; set; }
		public GridCoreSmart<TRAutDepoDto> ListaDepositos { get; set; }
		public GridCoreSmart<AnalizarAutOrdenDeRepartoDto> ListaAnalizaAut { get; set; }
	}
}
