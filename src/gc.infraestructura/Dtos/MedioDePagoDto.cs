
namespace gc.infraestructura.Dtos
{
	public class MedioDePagoDto : Dto
	{
		public string Ins_Id { get; set; } = string.Empty;
		public string Ins_Desc { get; set; } = string.Empty;
		public string Ins_Lista { get; set; } = string.Empty;
		public string? Ins_Dato1_Desc { get; set; }
		public string? Ins_Dato2_Desc { get; set; }
		public string? Ins_Dato3_Desc { get; set; }
		public char Ins_Detalle { get; set; }
		public decimal Ins_Comision { get; set; } = 0.00M;
		public decimal Ins_Comision_Fija { get; set; } = 0.00M;
		public string? Ins_Razon_Social { get; set; }
		public string? Ins_Cuit { get; set; }
		public decimal Ins_Ret_Gan { get; set; }
		public decimal Ins_Ret_Ib { get; set; }
		public decimal Ins_Ret_Iva { get; set; }
		public char Ins_Arqueo { get; set; }
		public char Ins_Tiene_Vto { get; set; }
		public char Ins_Vigente { get; set; }
		public string? Ctaf_Id_Link { get; set; }
		public string Tcf_Id { get; set; } = string.Empty;
		public string Tcf_Desc { get; set; } = string.Empty;
		public string? Ins_Id_Pos { get; set; }
		public string? Ins_Id_Pos_Ctls { get; set; }
	}
}
