using gc.infraestructura.Dtos.Almacen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
    public class CuentaAbmModel
    {
        public SelectList ComboAfip { get; set; }
        public SelectList ComboNatJud { get; set; }
        public SelectList ComboTipoDoc { get; set; }
        public SelectList ComboIngBruto { get; set; }
		public SelectList ComboProvincia { get; set; }
		public SelectList ComboDepartamento { get; set; }
		public SelectList ComboTipoCuentaBco { get; set; }
		public SelectList ComboTipoNegocio { get; set; }
		public SelectList ComboListaDePrecios { get; set; }
		public SelectList ComboTipoCanal { get; set; }
		public SelectList ComboVendedores { get; set; }
		public CuentaABMDto Cliente { get; set; }

        public CuentaAbmModel()
        { 
            Cliente = new CuentaABMDto();
        }
    }
}
