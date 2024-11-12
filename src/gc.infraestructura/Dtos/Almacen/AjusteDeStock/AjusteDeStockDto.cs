using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock
{
	public class AjusteDeStockDto : Dto
	{
        public SelectList ComboDepositos { get; set; }
        public SelectList ComboBoxes { get; set; }
        public SelectList ComboMotivos { get; set; }
        public GridCore<ProductoAAjustarDto> ProductosAAjustar { get; set; }

        public AjusteDeStockDto()
        {
            ProductosAAjustar = new GridCore<ProductoAAjustarDto>();
        }
	}
}
