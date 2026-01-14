namespace gc.sitio.Areas.ABMs.Models
{
	public class MPOpcionCuotaAbmValidationModel
	{
        public string ins_id { get; set; }
        public int pos_plan { get; set; }
		public string pos_desc { get; set; }
		public decimal recargo { get; set; }
		public int opcion { get; set; }
	}
}
