namespace gc.infraestructura.Dtos.Inventario.Dto
{
    public class InventarioConteoDto
    {
        public string p_id { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public string box_id { get; set; } = string.Empty;
        public int carga_nro { get; set; }
        public string carga_des { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public int invd_unidad_pres { get; set; } 
        public int invd_bulto { get; set; }
        public decimal invd_unidad_suelta { get; set; }
        public decimal invd_cantidad { get; set; }
    }
}
