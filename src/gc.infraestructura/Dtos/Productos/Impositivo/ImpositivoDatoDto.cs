namespace gc.infraestructura.Dtos.Productos.Impositivo
{
    public class ImpositivoDatoDto
    {
        public string p_id { get; set; } =string.Empty;
        public string p_desc { get; set; }= string.Empty;
        public string cta_id { get; set; }= string.Empty;
        public string cta_denominacion { get; set; }= string.Empty;
        public string pg_id { get; set; }= string.Empty;
        public string pg_desc { get; set; }= string.Empty;
        public string sec_id { get; set; }= string.Empty;
        public string sec_desc { get; set; }= string.Empty;
        public string rubg_id { get; set; }= string.Empty;
        public string rubg_desc { get; set; }= string.Empty;
        public string rub_id { get; set; }= string.Empty;
        public string rub_desc { get; set; }= string.Empty;
        public string iva_situacion { get; set; }= string.Empty;
        public decimal iva_alicuota { get; set; }
        public decimal in_alicuota { get; set; }
        public decimal p_pcosto { get; set; }
        public string infoImp
        {
            get { return $"{iva_situacion??""}-{iva_alicuota}"; }
        }
    }
}
