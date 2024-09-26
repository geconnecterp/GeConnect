
namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRAutAnalizaDto : Dto
	{
        public string adm_id { get; set; }=string.Empty;
        public string adm_nombre { get; set; }=string.Empty;
        public string p_id { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public decimal pedido { get; set; }
        public decimal stk { get; set; }
        public decimal stk_adm { get; set; }
        public string box_id { get; set; } = string.Empty;
        public string depo_id { get; set; }=string.Empty;
        public string depo_nombre { get; set; } = string.Empty;
        public decimal a_transferir { get; set; }
        public decimal a_transferir_box { get; set; }
		public string fv { get; set; } = string.Empty;
		public string pi_compte { get; set; } = string.Empty;
        public int unidad_palet { get; set; }
        public decimal palet { get; set; }
        public int autorizacion { get; set; }
        public bool p_sustituto { get; set; }
        public string p_id_sustituto { get; set; }=string.Empty;
        public string nota { get; set; } = string.Empty;
    }
}