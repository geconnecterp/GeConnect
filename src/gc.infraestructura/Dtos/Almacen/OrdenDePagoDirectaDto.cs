using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.infraestructura.Dtos.Almacen
{
	public class OrdenDePagoDirectaDto : Dto
	{
		public ComprobanteDto opd { get; set; } = new ComprobanteDto();
		public List<ConceptoFacturadoDto> listaConceptoFacturado { get; set; } = [];
		public List<OtroTributoDto> listaOtrosTributos { get; set; } = [];
	}
}
