namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class FeResDto
    {
        public string tco_id { get; set; }=string.Empty;
        public string cm_compte { get; set; }=string.Empty;
        public string cm_repetido { get; set; }=string.Empty;
        public string cm_compte_hasta { get; set; }=string.Empty;
        public string adm_id { get; set; }=string.Empty;
        public string cta_id { get; set; }=string.Empty;
        public string dia_movi { get; set; }=string.Empty;
        public string cm_nombre { get; set; }=string.Empty;
        public string cm_domicilio { get; set; }=string.Empty;
        public string cm_cuit { get; set; }=string.Empty;
        public DateTime cm_fecha { get; set; }
        public string cm_libro_iva { get; set; }=string.Empty;
        public decimal cm_gravado { get; set; }
        public decimal cm_no_gravado { get; set; }
        public decimal cm_exento { get; set; }
        public decimal cm_dto { get; set; }
        public decimal cm_dto_porc { get; set; }
        public decimal cm_ii { get; set; }
        public decimal cm_iva { get; set; }
        public decimal cm_percepciones { get; set; }
        public decimal cm_total { get; set; }
        public string mon_codigo { get; set; }
        public string usu_id { get; set; }
        public string mv_id { get; set; }=string.Empty;
        public string afip_id { get; set; }=string.Empty;
        public string caja_nro_proceso { get; set; }=string.Empty;
        public string caja_nro_cierre { get; set; }=string.Empty;
        public string caja_id { get; set; }=string.Empty;   
        public string lp_id { get; set; }=string.Empty;
        public string ve_id { get; set; }=string.Empty;
        public string cm_cae { get; set; }=string.Empty;
        public DateTime? cm_cae_vto { get; set; }
        public string tco_letra { get; set; }=string.Empty;
        public string tco_desc { get; set; }=string.Empty;
        public string adm_direccion { get; set; }=string.Empty;
        public string emp_razon_social { get; set; }=string.Empty;
        public string emp_domicilio { get; set; }=string.Empty;
        public string emp_cuit { get; set; }=string.Empty;
        public string emp_ib_nro { get; set; }=string.Empty;
        public DateTime emp_inicio_act { get; set; }
        public string afip_desc_emp { get; set; }=string.Empty;
        public string afip_desc_cli { get; set; }=string.Empty;
        public string compte_ori { get; set; }=string.Empty;
        public string sorteo { get; set; }=string.Empty;
    }
}
