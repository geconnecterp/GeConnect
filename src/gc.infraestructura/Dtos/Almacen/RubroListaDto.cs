
namespace gc.infraestructura.Dtos.Almacen
{
    public class RubroListaDto : Dto
    {
        public RubroListaDto()
        {
            Rub_Desc=string.Empty;
            Rub_Id =string.Empty;
        }
        public string Rub_Id { get; set; }=string.Empty;
        public string Rub_Desc { get; set; }= string.Empty;
        public string Rub_Lista { get; set; } = string.Empty;
    }

    public class RubroListaABMDto : RubroListaDto
    {
        public char Rub_Feteado { get; set; }
        public char Rub_Ctlstk { get; set; }
        public string Rubg_Id { get; set; } = string.Empty;
        public string Rubg_Desc { get; set; } = string.Empty;
		public string Rubg_Lista { get; set; } = string.Empty;
		public string Sec_Id { get; set; } = string.Empty;
		public string Sec_Desc { get; set; } = string.Empty;
		public bool Rub_Feteado_Activa
		{
			get
			{
				if (char.IsWhiteSpace(Rub_Feteado) || string.IsNullOrWhiteSpace(char.ToString(Rub_Feteado)))
					return false;
				return Rub_Feteado == 'S';
			}
			set { rub_Feteado_Activa = value; }
		}
		private bool rub_Feteado_Activa;
		public bool Rub_Ctlstk_Activa
		{
			get
			{
				if (char.IsWhiteSpace(Rub_Ctlstk) || string.IsNullOrWhiteSpace(char.ToString(Rub_Ctlstk)))
					return false;
				return Rub_Ctlstk == 'S';
			}
			set { rub_Ctlstk_Activa = value; }
		}
		private bool rub_Ctlstk_Activa;
	}

	public class RubroItemListaDto : Dto
	{
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public string rub_lista { get; set; } = string.Empty;
		public string rubg_id { get; set; } = string.Empty;
		public string rubg_desc { get; set; } = string.Empty;
		public string rubg_lista { get; set; } = string.Empty;
		public string sec_id { get; set; } = string.Empty;
		public string sec_desc { get; set; } = string.Empty;
	}
}
