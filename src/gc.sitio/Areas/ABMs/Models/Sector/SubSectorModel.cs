namespace gc.sitio.Areas.ABMs.Models
{
	public class SubSectorModel
	{
		public string Rubg_Id { get; set; } = string.Empty;
		public string Rubg_Desc { get; set; } = string.Empty;
		public string Rubg_Lista { get; set; } = string.Empty;
		public string Sec_Id { get; set; } = string.Empty;
		public string Sec_Desc { get; set; } = string.Empty;
		public char Rubg_Actu { get; set; } = char.MinValue;
	}
}
