namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class CajaOpeConfirmarReq
    {
        public string caja_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
        public string lp_id { get; set; } = string.Empty;
        public string caja_nro_proceso { get; set; } = string.Empty;
        public int caja_nro_cierre { get; set; }

        public string cta_id { get; set; } = string.Empty;
        public decimal ctac_dto { get; set; }
        public string co_tipo { get; set; } = "CR";
        public string ctc_id { get; set; } = string.Empty;

        public string tco_letra { get; set; } = string.Empty;
        public string tco_id_ori { get; set; } = string.Empty;
        public string cm_compte_ori { get; set; } = string.Empty;

        public string afip_id { get; set; } = string.Empty;
        public string tdoc_id { get; set; } = string.Empty;
        public string cta_documento { get; set; } = string.Empty;
        public string cta_denominacion { get; set; } = string.Empty;
        public string cta_domicilio { get; set; } = string.Empty;

        public string ve_id { get; set; } = string.Empty;

        public string json_p { get; set; } = string.Empty;
        public string json_valores { get; set; } = string.Empty;
        public string json_cancela { get; set; } = string.Empty;
        public string json_union { get; set; } = string.Empty;
        public string json_subtotal { get; set; } = string.Empty;
        public string json_sorteo { get; set; } = string.Empty;
    }
}
