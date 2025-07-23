namespace gc.infraestructura.Dtos.Productos
{
    public class ProductoRequestPvtaBase
    {       
        public decimal p_pcosto { get; set; }
        public decimal lp_prevision_tot { get; set; }
        public decimal lp_prevision_pin { get; set; }
        public decimal tp_margen { get; set; }
        public char iva_situacion { get; set; } = 'N';
        public decimal iva_alicuota { get; set; }
        public decimal in_alicuota { get; set; } = 0;        
    }

    public class ProductoRequestPvtaLista
    {
        public decimal p_pcosto { get; set; }
        public decimal p_pneto_base { get; set; }
        public decimal lp_porc_mg { get; set; }
        public char iva_situacion { get; set; } = 'N';
        public decimal iva_alicuota { get; set; }
        public decimal in_alicuota { get; set; } = 0;
    }

    public class ProductoResponsePVta
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
