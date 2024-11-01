namespace gc.infraestructura.Dtos.Deposito
{
    public class DepositoInfoStkDto
    {
        public string P_id { get; set; }=string.Empty;
        public string P_desc { get; set; } = string.Empty;
        public string Depo_id { get; set; } = string.Empty;
        public string Depo_nombre { get; set; } = string.Empty;
        public string? Up_id { get; set; }
        public string? Up_desc { get; set; }
        public string Cta_id { get; set; } = string.Empty;
        public string Cta_denominacion { get; set; } = string.Empty;
        public string Rub_id { get; set; } = string.Empty;
        public string Rub_desc { get; set; } = string.Empty;    
        public decimal Ps_stk { get; set; }
        public decimal Ps_bulto { get; set; }
        public DateTime? Vto { get; set; }       
    }
}
