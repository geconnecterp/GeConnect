
namespace gc.infraestructura.Dtos
{
	public class TipoRetGananciaDto : Dto
	{
        public string rgan_id { get; set; } = string.Empty;
        public string rgan_desc { get; set; } = string.Empty;
		public string rgan_lista { get; set; } = string.Empty;
		public char imp_acu_mensual { get; set; }
        public decimal rgan_imp_no_ret { get; set; } = 0.000M;
    }
}
