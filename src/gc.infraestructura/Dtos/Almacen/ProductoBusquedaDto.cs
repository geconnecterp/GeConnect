namespace gc.infraestructura.Dtos.Almacen
{
    public class ProductoBusquedaDto : Dto
    {
        public string P_id { get; set; }=string.Empty;
        public string P_id_barrado { get; set; } = string.Empty;
        public string P_id_prov { get; set; } = string.Empty;
        public string P_m_marca { get; set; } = string.Empty;
        public string P_m_desc { get; set; } = string.Empty;
        public string P_m_capacidad { get; set; } = string.Empty;
        public string P_desc { get; set; } = string.Empty;
        public string P_alta_rotacion { get; set; } = string.Empty;
        public string P_con_vto { get; set; } = string.Empty;
        public string P_peso { get; set; } = string.Empty;
        public string Up_id { get; set; } = string.Empty; //= 00 => decimal -> acepto lo que traiga (3 decimales)
        public string Up_tipo { get; set; } = string.Empty;
        public string Rub_id { get; set; } = string.Empty;
        public string Rub_desc { get; set; } = string.Empty;
        public string Rub_feteado { get; set; } = string.Empty;
        public string Rubg_id { get; set; } = string.Empty;
        public string Rubg_desc { get; set; } = string.Empty;
        public string Cta_id { get; set; } = string.Empty;
        public string Cta_denominacion { get; set; } = string.Empty;
        public string Pg_id { get; set; } = string.Empty;
        public string Pg_desc { get; set; } = string.Empty;
        public string P_activo { get; set; } = string.Empty;
        public string P_balanza { get; set; } = string.Empty;
        public string P_balanza_dvto { get; set; } = string.Empty;
        public string P_balanza_id { get; set; } = string.Empty;
        public string Adm_may_excluye { get; set; } = string.Empty;
        public string Adm_min_excluye { get; set; } = string.Empty;
        public string Lp_id_default { get; set; } = string.Empty;
        public string In_alicuota { get; set; } = string.Empty;
        public string P_in { get; set; } = string.Empty;
        public string Iva_situacion { get; set; } = string.Empty;
        public string Iva_alicuota { get; set; } = string.Empty;
        public string P_iva { get; set; } = string.Empty;
        public string P_pneto { get; set; } = string.Empty;
        public string P_pvta_oferta { get; set; } = string.Empty;
        public string P_pvta { get; set; } = string.Empty;
        public string P_pcosto { get; set; } = string.Empty;
        public string P_pcosto_repo { get; set; } = string.Empty;
        public string P_unidad_vta { get; set; } = string.Empty;
        public string P_unidad_pres { get; set; } = string.Empty;
        public string Cli_dto { get; set; } = string.Empty;
        public string Es_oferta { get; set; } = string.Empty;
        public string Msj { get; set; } = string.Empty;
        public int Item { get; set; }
        public int Bulto { get; set; }
        public decimal Unidad { get; set; }
        public decimal Cantidad { get; set; }
        public string oc_compte { get; set; } = string.Empty;
        public short p_con_vto_min { get; set; }
        public DateTime? p_con_vto_ctl { get; set; }
    }
}
