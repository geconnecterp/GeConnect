namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class RendicionCargaRequestDto
    {
        public string caja_nro_proceso { get; set; } = string.Empty;
        public int caja_nro_cierre { get; set; }
        public string caja_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
        public string json_rendiciones { get; set; } = string.Empty;
    }
}
