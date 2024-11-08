namespace gc.infraestructura.Dtos.Box
{
    public class BoxInfoMovStkDto
    {
        public string P_id { get; set; } = string.Empty;//
        public string P_desc { get; set; } = string.Empty;//
        public DateTime Sm_Fecha { get; set; }
        public string Sm_concepto { get; set; } = string.Empty;
        public string Tco_id { get; set; } = string.Empty;
        public string cm_compte { get; set; } = string.Empty;   
        public string Depo_id { get; set; } = string.Empty;//
        public string Depo_nombre { get; set; } = string.Empty;//
        public string Up_id { get; set; } = string.Empty;//
        public string Up_desc { get; set; } = string.Empty;//       
        public string Box_id { get; set; } = string.Empty;//
        public string Sm_tipo { get; set; }= string.Empty;
        public string Sm_desc { get; set; } = string.Empty;
        public decimal Sm_es { get; set; } 
        public decimal Sm_es_b { get; set; } 
        public decimal Sm_stk { get; set; } 
        public decimal Sm_stk_b { get; set; } 

    }
}
