using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Request;

namespace gc.infraestructura.Dtos.Almacen
{
	public class OrdenDePagoDirectaDto : Dto
	{
		public ComprobanteDto opd { get; set; } = new ComprobanteDto();
		public List<ConceptoFacturadoEnOPDDto> listaConceptoFacturado { get; set; } = [];
		public List<OtroTributoEnOPDDto> listaOtrosTributos { get; set; } = [];

		public OrdenDePagoDirectaDto()
		{
			opd = new ComprobanteDto();
			listaConceptoFacturado = [];
			listaOtrosTributos = [];
		}
	}
}
