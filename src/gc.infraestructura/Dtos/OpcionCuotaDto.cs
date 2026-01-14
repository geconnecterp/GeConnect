
namespace gc.infraestructura.Dtos
{
	public class OpcionCuotaDto : Dto
	{
		public string Ins_Id { get; set; } = string.Empty;
		public int Opcion { get; set; } = 0;
		public string Pos_Plan { get; set; } = string.Empty;
		public string Pos_Desc { get; set; } = string.Empty;
		public decimal Recargo { get; set; } = 0.00M;
	}
}
