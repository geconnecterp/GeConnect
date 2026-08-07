namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class AnulacionCobranzaBuscarRequestDto
    {
        public string caja_nro_proceso { get; set; } = string.Empty;
        public int caja_nro_cierre { get; set; }
        public string cta_id { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string adm_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
    }
}
