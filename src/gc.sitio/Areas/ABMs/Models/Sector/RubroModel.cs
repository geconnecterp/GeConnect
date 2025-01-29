using gc.infraestructura.Dtos.Almacen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class RubroModel
	{
		public string Rub_Id { get; set; } = string.Empty;
		public string Rub_Desc { get; set; } = string.Empty;
		public string Rub_Lista { get; set; } = string.Empty;
		public char Rub_Feteado { get; set; }
		public char Rub_Ctlstk { get; set; }
		public string Rubg_Id { get; set; } = string.Empty;
		public string Rubg_Desc { get; set; } = string.Empty;
		public string Rubg_Lista { get; set; } = string.Empty;
		public string Sec_Id { get; set; } = string.Empty;
		public string Sec_Desc { get; set; } = string.Empty;

		public SelectList ComboSubSector { get; set; }

		public RubroModel() { }

		public RubroModel(List<RubroListaABMDto> listaRubro)
		{

		}
	}
}
