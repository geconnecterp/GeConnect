namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class ValoresInsResDto
    {
        public string ins_id { get; set; } = string.Empty;
        public string ins_desc { get; set; } = string.Empty;
        public string mon_codigo { get; set; } = string.Empty;
        public string ins_detalle { get; set; } = string.Empty;
        public string? ins_dato1_desc { get; set; }
        public string? ins_dato2_desc { get; set; }
        public string? ins_dato3_desc { get; set; }
        public string tcf_id { get; set; } = string.Empty;
        public string ins_tiene_vto { get; set; } = string.Empty;
        public string ins_arqueo { get; set; } = string.Empty;
        public string ins_vuelto { get; set; } = string.Empty;
        public string ins_vigente { get; set; } = string.Empty;
        public string? ins_razon_social { get; set; }
        public string? ins_cuit { get; set; }
        public decimal ins_comision { get; set; }
        public decimal ins_comision_fija { get; set; }
        public decimal ins_ret_gan { get; set; }
        public decimal ins_ret_ib { get; set; }
        public decimal ins_ret_iva { get; set; }
        public string? ctaf_id_link { get; set; }
        public short? ins_dias_acre { get; set; }
        public string? inse_empresa { get; set; }
        public string? ins_id_barrado { get; set; }
        public string? ins_id_archivo { get; set; }
        public string? ins_id_pos { get; set; }
        public string? ins_id_pos_ctls { get; set; }
    }
}
