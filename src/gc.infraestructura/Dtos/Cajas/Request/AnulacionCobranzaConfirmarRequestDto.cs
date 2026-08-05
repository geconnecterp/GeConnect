namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class AnulacionCobranzaConfirmarRequestDto
    {
        public string caja_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
        public string caja_nro_proceso_anu { get; set; } = string.Empty;
        public int caja_nro_cierre_anu { get; set; }
        public int caja_nro_operacion_anu { get; set; }
        public string cta_id { get; set; } = string.Empty;
        public string usu_id_autoriza { get; set; } = string.Empty;
    }
}
