namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class CierreCajaRequestDto
    {
        public string caja_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
        public string json_rendiciones { get; set; } = "[]";
    }
}
