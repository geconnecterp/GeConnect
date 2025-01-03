namespace gc.infraestructura.Dtos.Productos
{
    public class LimiteStkDto
    {
        public string p_id { get; set; }=string.Empty;
        public string p_stk_min { get; set; }=string.Empty ;
        public string p_stk_max { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
        public string adm_nombre { get; set; } = string.Empty;
        public string adm_lista { get; set; } = string.Empty;
    }
}
