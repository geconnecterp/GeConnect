namespace gc.sitio.Areas.ABMs.Models
{
	public class OpcionCuotaModel
	{
		public string Ins_Id { get; set; } = string.Empty;
		public int Cuota { get; set; } = 0;
		public decimal Recargo { get; set; } = 0.00M;
		public string? Pos_Plan { get; set; }
	}
}
