namespace gc.infraestructura.Dtos.Almacen.Tr
{
    public class TiListaProductoDto
    {
        public string Ti { get; set; } = string.Empty;
        public string Rub_id { get; set; } = string.Empty;
        public string Rub_desc { get; set; } = string.Empty;
        public string Rubg_id { get; set; } = string.Empty;
        public string Rubg_desc { get; set; } = string.Empty;
        public string Box_id { get; set; } = string.Empty;
        public string Depo_id { get; set; } = string.Empty;
        public string Depo_nombre { get; set; } = string.Empty;
        public string P_id { get; set; } = string.Empty;
        public string P_desc { get; set; } = string.Empty;
        public decimal Colectado { get; set; }
        public decimal Pedido { get; set; }
        public short Bulto { get; set; }
        public decimal   Us { get; set; }
        public short Unidad_pres { get; set; }
        public string Nota { get; set; } = string.Empty;    

    }
}
