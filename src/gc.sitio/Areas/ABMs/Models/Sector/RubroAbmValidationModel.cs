namespace gc.sitio.Areas.ABMs.Models
{
	public class RubroAbmValidationModel
	{
        public string rub_id { get; set; } = string.Empty;
        public string rub_desc { get; set; } = string.Empty;
		public string rub_lista { get; set; } = string.Empty;
		public char rub_feteado { get; set; }
        public char rub_ctlstk { get; set; }
        public string rubg_id { get; set; } = string.Empty;
		public string rubg_desc { get; set; } = string.Empty;
		public string rubg_lista { get; set; } = string.Empty;
		public string sec_id { get; set; } = string.Empty;
		public string sec_desc { get; set; } = string.Empty;
	}
}
