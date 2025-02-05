
namespace gc.api.core.Entidades
{
	public class PlanContable : EntidadBase
	{
        public string Ccb_Id { get; set; } = string.Empty;
        public string Ccb_Desc { get; set; } = string.Empty;
		public string Ccb_Lista { get; set; } = string.Empty;
		public char Ccb_Tipo { get; set; }
        public string Ccb_Id_Padre { get; set; } = string.Empty;
		public char Ccb_Ajuste_Inflacion { get; set; }
        public char Ccb_Actu { get; set; }
        public decimal? ccb_saldo { get; set; }
    }
}
