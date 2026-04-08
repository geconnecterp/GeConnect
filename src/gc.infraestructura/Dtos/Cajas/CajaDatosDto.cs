namespace gc.infraestructura.Dtos.Cajas
{
    public class CajaDatosDto
    {
        public string caja_id { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
        public string depo_id { get; set; } = string.Empty;
        public DateTime? dia_movi { get; set; }
        public string usu_id { get; set; } = string.Empty;
        public string caja_nombre { get; set; } = string.Empty;
        public string caja_estado { get; set; } = string.Empty;
        public string caja_habilitadas { get; set; } = string.Empty;
        public string caja_modalidad { get; set; } = string.Empty;
        public DateTime? caja_apertura { get; set; }
        public DateTime? caja_cierre { get; set; }
        public int? caja_nro_proceso { get; set; }
        public int? caja_nro_cierre { get; set; }
        public int? caja_nro_operacion { get; set; }
        public string caja_activa { get; set; } = string.Empty;
        public string caja_manual { get; set; } = string.Empty;
        public string caja_cae { get; set; } = string.Empty;
        public DateTime? caja_cae_vto { get; set; }

        public bool min { get; set; }
        public string lp_id_min { get; set; } = string.Empty;
        public decimal? lp_id_min_porc { get; set; }

        public bool may { get; set; }
        public string lp_id_may { get; set; } = string.Empty;
        public decimal? lp_id_may_porc { get; set; }

        public bool dis { get; set; }
        public string lp_dis_may { get; set; } = string.Empty;
        public decimal? lp_id_dis_porc { get; set; }

        public string ctrl_id { get; set; } = string.Empty;
        public string ctrl_descripcion { get; set; } = string.Empty;
        public string ocx_marca { get; set; } = string.Empty;
        public int? copias { get; set; }
        public decimal? nro_ali_iva { get; set; }
        public decimal? importe_max_b { get; set; }
        public decimal? importe_max_b_fs { get; set; }
    }
}
