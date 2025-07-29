namespace gc.infraestructura.Dtos.Productos
{
    public class ProductoCPConfirmar:TPProducto
    {
        public string p_id { get; set; } = string.Empty;
        public List<TPProducto> Listas { get; set; } = [];
    }

    public class TPProducto
    {
        public string lp_id { get; set; } = string.Empty;
        public decimal tp_plista { get; set; }
        public decimal tp_dto1 { get; set; }
        public decimal tp_dto2 { get; set; }
        public decimal tp_dto3 { get; set; }
        public decimal tp_dto4 { get; set; }
        public decimal tp_dto_pa { get; set; }
        public decimal tp_porc_flete { get; set; }
        public string tp_boni { get; set; } = string.Empty;
        public decimal tp_pcosto { get; set; }
        public decimal tin_alicuota { get; set; }
        public decimal tp_margen { get; set; }
        public decimal tp_margen_vta { get; set; }
        public decimal tp_pneto { get; set; }
        public decimal tp_iva { get; set; }
        public decimal tp_in { get; set; }
        public decimal tp_pvta { get; set; }
    }
}
