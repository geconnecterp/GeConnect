namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class CtaCteResponseDto
    {
        public string cta_id { get; set; } = string.Empty;
        public string dia_movi { get; set; } = string.Empty;
        public string tco_id { get; set; } = string.Empty;
        public string cm_compte { get; set; } = string.Empty;
        public int cm_compte_cuota { get; set; }
        public DateTime cv_fecha_vto { get; set; }
        public decimal cv_importe { get; set; }
        public decimal cv_importe_ori { get; set; }
        public string cv_concepto { get; set; } = string.Empty;
        public string ve_id { get; set; } = string.Empty;
        public string ccb_id { get; set; } = string.Empty;
        public string ctacte { get; set; } = string.Empty;
        public string carga { get; set; } = string.Empty;
        public string carga_obligatoria { get; set; } = string.Empty;
    }
}
