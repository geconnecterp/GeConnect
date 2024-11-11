
namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock
{
	public class ProductoAAjustarDto : Dto
	{
        public string p_id { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public string id_prov { get; set; } = string.Empty;
        public decimal as_stock { get; set; } = 0.000M;
        public decimal as_ajuste { get; set; } = 0.000M;
        public decimal as_resultado { get; set; } = 0.000M;
    }
}
