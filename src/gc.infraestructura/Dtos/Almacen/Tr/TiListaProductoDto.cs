namespace gc.infraestructura.Dtos.Almacen.Tr
{
    public class TiListaProductoDto : Dto, IProductoConUnidad
	{
        public short Item { get; set; }
        public decimal Colectado_otros { get; set; }
        public string Resultado { get; set; } = "PE";
        public string Resultado_msj { get; set; } = string.Empty;
        public string Remplazo_box { get; set; } = string.Empty;
        public string Remplazo_p_id { get; set; } = string.Empty;
        public string Remplazo_usu_id { get; set; } = string.Empty;
        public decimal Colectado_x_box { get; set; }
        public decimal Colectado_x_p { get; set; }
        public decimal Colectado_remplazo { get; set; }
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
        public string up_id { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public bool PermiteDecimales => up_tipo == "P";
		public string Remplazo { get; set; } = "N";
        public decimal Colectado { get; set; }
        public decimal Pedido { get; set; }
        public short Bulto { get; set; }
        public decimal   Us { get; set; }
        public short Unidad_pres { get; set; }
        public string Nota { get; set; } = string.Empty;    

    }
}
