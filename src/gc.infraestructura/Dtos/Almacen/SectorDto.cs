
namespace gc.infraestructura.Dtos.Almacen
{
	public class SectorDto : Dto
	{
		public string Sec_Id { get; set; } = string.Empty;
		public string Sec_Desc { get; set; } = string.Empty;
		public string Sec_Lista { get; set; } = string.Empty;
		public char Sec_Prefactura { get; set; } = char.MinValue;
		public bool Sec_Prefa
		{
			get
			{
				if (char.IsWhiteSpace(Sec_Prefactura) || string.IsNullOrWhiteSpace(char.ToString(Sec_Prefactura)))
					return false;
				return Sec_Prefactura == 'S';
			}
			set { sec_Prefa = value; }
		}
		private bool sec_Prefa;
	}
}
