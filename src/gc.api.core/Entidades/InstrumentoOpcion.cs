
namespace gc.api.core.Entidades
{
	public class InstrumentoOpcion : EntidadBase
	{
		public string Ins_Id { get; set; } = string.Empty;
		public int Cuota { get; set; } = 0;
		public decimal Recargo { get; set; } = 0.00M;
        public string? Pos_Plan { get; set; }
    }
}
