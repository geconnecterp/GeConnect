
namespace gc.infraestructura.Dtos.ABM
{
	public class MedioDePagoABMDto : Dto
	{
		public string Ins_Id { get; set; } = string.Empty;
		public string Ins_Desc { get; set; } = string.Empty;
		public string Ins_Lista { get; set; } = string.Empty;
        public string Mon_Codigo { get; set; } = string.Empty;
		public string? Ins_Dato1_Desc { get; set; } = string.Empty;
		public string? Ins_Dato2_Desc { get; set; } = string.Empty;
		public string? Ins_Dato3_Desc { get; set; } = string.Empty;
		public char Ins_Detalle { get; set; } = char.MinValue;
		public decimal Ins_Comision { get; set; } = 0.00M;
		public decimal Ins_Comision_Fija { get; set; } = 0.00M;
		public string? Ins_Razon_Social { get; set; } = string.Empty;
		public string? Ins_Cuit { get; set; } = string.Empty;
		public decimal Ins_Ret_Gan { get; set; } = 0.00M;
		public decimal Ins_Ret_Ib { get; set; } = 0.00M;
		public decimal Ins_Ret_Iva { get; set; } = 0.00M;
		public char Ins_Arqueo { get; set; } = char.MinValue;
		public char Ins_Tiene_Vto { get; set; } = char.MinValue;
		public char Ins_Vigente { get; set; }
        public bool Ctaf_Id_Link_Check { get; set; }
        public string? Ctaf_Id_Link { get; set; } = string.Empty;
		public string Tcf_Id { get; set; } = string.Empty;
		public string Tcf_Desc { get; set; } = string.Empty;
		public string? Ins_Id_Pos { get; set; } = string.Empty;
		public string? Ins_Id_Pos_Ctls { get; set; } = string.Empty;
		public bool Ins_Activa
		{
			get
			{
				if (char.IsWhiteSpace(Ins_Vigente) || string.IsNullOrWhiteSpace(char.ToString(Ins_Vigente)))
					return false;
				return Ins_Vigente == 'S';
			}
			set { ins_Activa = value; }
		}
		private bool ins_Activa;
	}
}
