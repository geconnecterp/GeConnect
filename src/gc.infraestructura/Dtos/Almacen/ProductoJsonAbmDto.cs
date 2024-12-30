using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Almacen
{
    public class ProductoJsonAbmDto
    {
        public string p_id { get; set; }//*
        public string p_m_marca { get; set; }//*
        public string p_m_desc { get; set; }//*
        public string p_m_capacidad { get; set; }//*
        public string p_desc { get; set; }//*
        public char? p_alta_rotacion { get; set; }//*
        public string p_id_prov {  get; set; }//*
        public char? p_con_vto { get; set; }//*
        public short p_con_vto_min { get; set; }//*
        public decimal? p_peso { get; set; }//*
        public char p_elaboracion { get; set; }//
        public char p_materia_prima { get; set; }//
        public string up_id { get; set; }//
        public string up_desc { get; set; }
        public string up_lista { get; set; }
        public string rub_id { get; set; }
        public string rub_desc { get; set; }
        public string rub_lista { get; set; }
        public string cta_id { get; set; }
        public string cta_denominacion { get; set; }
        public string cta_lista { get; set; }
        public string pg_id { get; set; }
        public string pg_lista { get; set; }
        public decimal in_alicuota { get; set; }
        public decimal iva_alicuota { get; set; }
        public char iva_situacion { get; set; }
        public DateTime? p_modi { get; set; }
        public char? p_actu { get; set; }
        public char p_activo { get; set; }
        public char? p_balanza { get; set; }//*

        public char? adm_min_excluye { get; set; }
        public char? adm_may_excluye { get; set; }
        public char? pi_auto_excluye { get; set; }
        public char? oc_auto_excluye { get; set; }
        public string lp_id_default { get; set; }
        public string p_id_barrado_ean { get; set; }
        public int p_unidad_pres_ean { get; set; }
        public int p_unidad_x_bulto_ean { get; set; }
        public int p_bulto_x_piso_ean { get; set; }
        public int p_piso_x_pallet_ean { get; set; }
        public string p_id_barrado_dun { get; set; }
        public int p_unidad_pres_dun { get; set; }
        public int p_unidad_x_bulto_dun { get; set; }
        public int p_bulto_x_piso_dun { get; set; }
        public int p_piso_x_pallet_dun { get; set; }

    }
}
