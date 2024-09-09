namespace gc.infraestructura.Dtos.Almacen.Tr
{
    public class AutorizacionTIDto
    {
        public string Ti { get; set; } = string.Empty;
        public string Adm_id_des { get; set; } = string.Empty;
        public string Adm_nombre { get; set; } = string.Empty;
        public string Usu_id { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } 
        public string? Nota { get; set; } 
        public string? Tie_id { get; set; } 
        public string? Pi_compte { get; set; } 

    }
}
