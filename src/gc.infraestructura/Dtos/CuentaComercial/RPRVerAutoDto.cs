using gc.infraestructura.Dtos.Almacen.Rpr;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.CuentaComercial
{
	public class RPRVerAutoDto
	{
		public string Leyenda { get; set; } = string.Empty;
		public string Depo_id { get; set; } = string.Empty;
		public SelectList ComboDeposito { get; set; } = new SelectList(new List<Dto>());
		public List<RPRComptesDeRPDto> Comprobantes { get; set; } = new List<RPRComptesDeRPDto>();	
        public List<RPRxULDto> ConteosxUL { get; set; } = new List<RPRxULDto>();
        public string Rp { get; set; } = string.Empty;

		public RPRVerAutoDto() 
		{
			Comprobantes = [];
			ConteosxUL = [];
		}
    }
}
