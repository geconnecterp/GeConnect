namespace gc.infraestructura.Dtos.Almacen.Tr
{
    public class BoxRubProductoDto
    {
        public string Ti { get; set; }=string.Empty;
        public string? Depo_id { get; set; } 
        public string? Depo_nombre { get; set; } 
        public string? Box_id { get; set; } 
        public string? Rub_Id { get; set; }
        public string? Rub_desc { get; set; }
        public string? Rubg_Id { get; set; }
        public string? Rubg_desc { get; set; }
        public string Conteo { get; set; } = string.Empty;
        public string Ori { get; set; } = string.Empty;

    }
}
