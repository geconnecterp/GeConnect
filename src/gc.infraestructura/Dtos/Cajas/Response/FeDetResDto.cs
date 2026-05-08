namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class FeDetResDto
    {
        public string tco_id { get; set; }=string.Empty;
        public string cm_compte { get; set; }=string.Empty;
        public string cm_repetido { get; set; }=string.Empty;
        public string cmd_item { get; set; }=string.Empty;
        public string cm_hoja { get; set; }=string.Empty;
        public string p_id { get; set; }=string.Empty;
        public string p_desc { get; set; }=string.Empty;
        public decimal cmd_cantidad { get; set; }
        public decimal cmd_pcosto { get; set; }
        public decimal cmd_dto_porc { get; set; }
        public decimal cmd_dto { get; set; }
        public decimal in_alicuota { get; set; }
        public decimal cmd_in { get; set; }
        public string iva_situacion { get; set; }=string.Empty;
        public decimal iva_alicuota { get; set; }
        public decimal cmd_iva { get; set; }
        public decimal cmd_pneto { get; set; }
        public decimal cmd_mgn_imp { get; set; }
        public decimal cmd_mgn { get; set; }
        public decimal cmd_pvta { get; set; }
        public decimal cmd_subtotal { get; set; }
        public decimal cmd_ii { get; set; }
        public decimal cmd_boni { get; set; }
        public decimal cmd_subtotal_con_iva { get; set; }
        public string cmd_oferta { get; set; }=string.Empty;
        public string cmd_cmb { get; set; }=string.Empty;
        public string cmd_cmb_id { get; set; }=string.Empty;    
        public decimal cmd_cmb_dto { get; set; }
        public string cmd_cmb_cant { get; set; } = string.Empty;
        public string cmd_cmb_desc { get; set; }=string.Empty;
    }
}
