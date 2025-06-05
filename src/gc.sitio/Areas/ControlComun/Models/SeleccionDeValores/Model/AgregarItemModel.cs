using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.sitio.Areas.ControlComun.Models.SeleccionDeValores.Model
{
	public class AgregarItemModel
	{
		public List<ValoresDesdeObligYCredDto> DataObject { get; set; } = [];
		public string DataType { get; set; } = string.Empty;
	}
}
