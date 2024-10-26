
namespace gc.infraestructura.Dtos.Almacen.Response
{
	public class NCPICargaPedidoResponse
	{
        public int resultado { get; set; }
        public string resultado_msj { get; set; } = string.Empty;
        public int unidad_pres { get; set; }
        public decimal p_pcosto { get; set; } = 0.000M;
        public int bultos { get; set; } = 0;
        public int cantidad { get; set; } = 0;
        public decimal pallet { get; set; } = 0.000M;
    }
}
