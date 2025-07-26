using gc.infraestructura.Dtos.Almacen.AnulacionDeComprobante;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models.AnulacionDeComprobante
{
	public class NotasACuentaModel
	{
		public bool MostrarLeyenda { get; set; } = false;
		public bool MostrarGrilla { get; set; } = false;
		public GridCoreSmart<NotaACuentaDto> GrillaNotasACuenta { get; set; }
	}
}
