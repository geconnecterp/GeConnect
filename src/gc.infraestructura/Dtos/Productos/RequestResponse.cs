namespace gc.infraestructura.Dtos.Productos
{
    public class ProductoRequestPvtaLista
    {       
        public decimal p_pcosto { get; set; }
        public decimal lp_prevision_tot { get; set; }
        public decimal lp_prevision_pin { get; set; }
        public decimal tp_margen { get; set; }
        public char iva_situacion { get; set; } = 'N';
        public decimal iva_alicuota { get; set; }
        public decimal in_alicuota { get; set; } = 0;        
    }

    public class ProductoResponsePVtaLista
    {
        public decimal p_pneto { get; set; }
        public decimal p_pvta { get; set; }
        public decimal p_iva { get; set; }
        public decimal p_in { get; set; }
    }

    public class ProductoRequestPVtaMargen
    {
        public decimal p_pcosto { get; set; }
        public decimal lp_prevision_tot { get; set; }
        public decimal lp_prevision_pin { get; set; }
        public decimal p_pvta { get; set; }
        public char iva_situacion { get; set; } = 'N';
        public decimal iva_alicuota { get; set; }
        public decimal in_alicuota { get; set; } = 0;
    }

    public class ProductoResponsePVtaMargen
    {
        public decimal p_pneto { get; set; }
        public decimal p_margen { get; set; }
        public decimal p_iva { get; set; }
        public decimal p_in { get; set; }
    }

   
}
