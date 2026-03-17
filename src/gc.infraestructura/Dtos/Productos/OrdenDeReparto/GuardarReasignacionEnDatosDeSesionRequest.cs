using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Productos.OrdenDeReparto
{
	public class GuardarReasignacionEnDatosDeSesionRequest
	{
		public string orCompte { get; set; }
		public string pcCompte { get; set; }
		public List<ReasignacionDetalle> Detalle { get; set; }

	}
	public class ReasignacionDetalle
	{
		public string pId { get; set; }
		public decimal Cantidad { get; set; }
	}

}
