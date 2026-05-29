namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class ValoresNCResDto:Json_Union
    {
        
        public string? ctacte { get; set; }
        public string? carga { get; set; }
        public string? carga_obligatoria { get; set; }
    }

    public class Json_Union
    {
        public string? cta_id { get; set; }
        public string? dia_movi { get; set; }
        public string? tco_id { get; set; }
        public string? cm_compte { get; set; }
        public string? cm_compte_cuota { get; set; }
        public string? cv_fecha_vto { get; set; }
        public string? cv_importe { get; set; }
        public string? cv_importe_ori { get; set; }
        public string? cv_concepto { get; set; }
        public string? ve_id { get; set; }
        public string? ccb_id { get; set; }
    }
}
