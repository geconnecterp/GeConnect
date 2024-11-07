using gc.infraestructura.Dtos.Almacen.Rpr;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.CuentaComercial
{
	public class RPRVerAutoDto
	{
		public string Leyenda { get; set; } = string.Empty;
		public string Depo_id { get; set; } = string.Empty;
		public SelectList ComboDeposito { get; set; }
		public List<RPRComptesDeRPDto> Comprobantes { get; set; }
        public List<RPRxULDto> ConteosxUL { get; set; }
        public string Rp { get; set; } = string.Empty;

		public RPRVerAutoDto() 
		{
			Comprobantes = [];
			ConteosxUL = [];
		}
    }
}
