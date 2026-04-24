using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores
{
	//CH -> Cheque
	public class MedioDePagoCHModel : IMedioDePago
	{
		public SelectList ListaBcoCheqs { get; set; }
		public string BcoCheqsSeleccionado { get; set; } = string.Empty;
		public string NroCheque { get; set; } = string.Empty;
		public string Plaza { get; set; } = string.Empty;
		[DataType(DataType.Date)]
		public DateTime FechaVto { get; set; }
		public decimal Importe { get; set; }
	}
}
