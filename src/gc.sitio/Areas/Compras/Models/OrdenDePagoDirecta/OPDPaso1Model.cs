using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoDirecta
{
	public class OPDPaso1Model
	{
		public GridCoreSmart<OPDirectaObligacionesDto> GrillaObligaciones { get; set; }
		public SelectList listaTiposComptes { get; set; }
		public SelectList listaCondAfip { get; set; }
		public SelectList listaCuentaDirecta { get; set; }
		public SelectList listaMotivos { get; set; }
		public AgregarOPDRequest itemOPD { get; set; }
		public GridCoreSmart<ConceptoFacturadoDto> GrillaConceptosFacturados { get; set; }
		public GridCoreSmart<OtroTributoDto> GrillaOtrosTributos { get; set; }
		public GridCoreSmart<OrdenDeCompraConceptoDto> GrillaConcpetos { get; set; }
	}
}
