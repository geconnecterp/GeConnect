namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class AnulacionCobranzaResponseDto
    {
        public string caja_nro_proceso { get; set; } = string.Empty;
        public int caja_nro_cierre { get; set; }
        public int caja_nro_operacion { get; set; }
        public string rb_compte { get; set; } = string.Empty;
        public decimal co_cobranza { get; set; }
        public string caja_id { get; set; } = string.Empty;
        public string co_tipo { get; set; } = string.Empty;
        public DateTime co_fecha { get; set; }
        public string co_anulado { get; set; } = string.Empty;
        public string cta_id { get; set; } = string.Empty;
        public string comprobantes_cancelados { get; set; } = string.Empty;
    }
}
