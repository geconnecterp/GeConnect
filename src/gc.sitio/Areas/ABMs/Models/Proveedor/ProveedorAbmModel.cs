using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class ProveedorAbmModel
	{
		public SelectList ComboAfip { get; set; }
		public SelectList ComboNatJud { get; set; }
		public SelectList ComboTipoDoc { get; set; }
		public SelectList ComboIngBruto { get; set; }
		public SelectList ComboProvincia { get; set; }
		public SelectList ComboDepartamento { get; set; }
		public SelectList ComboTipoOpe { get; set; }
		public SelectList ComboTipoOc { get; set; }
		public SelectList ComboTipoGasto { get; set; }
		public SelectList ComboTipoRetGan { get; set; }
		public SelectList ComboTipoRetIB { get; set; }
		public ProveedorABMDto Proveedor { get; set; }
		public GridCore<CuentaFPDto> CuentaFormasDePago { get; set; }
		public GridCore<CuentaContactoDto> CuentaContactos { get; set; }
		public GridCore<CuentaObsDto> CuentaObs { get; set; }
		public GridCore<CuentaNotaDto> CuentaNota { get; set; }

		public ProveedorAbmModel()
		{
			Proveedor = new ProveedorABMDto();
		}
    }
}
