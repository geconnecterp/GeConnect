using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Productos
{
    public class ProductoDetalleDto
    {
        public string p_id { get; set; } = string.Empty;
        public string p_id_prov { get; set; } = string.Empty;
        public string p_id_barrado { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public string cta_id { get; set; } = string.Empty;
        public string pg_id { get; set; } = string.Empty;
        public string pg_desc { get; set; } = string.Empty;
        public string rub_id { get; set; } = string.Empty;
        public string rub_desc { get; set; } = string.Empty;
        #region p_activo
        public char p_activo { get; set; }
        private bool pActivo;
        public bool PActivo
        {
            get
            {
                if (char.IsWhiteSpace(p_activo) ||
                    string.IsNullOrWhiteSpace(char.ToString(p_activo)))
                    return false;
                return p_activo == 'S';
            }
            set
            {
                //P_Materia_Prima = value ? 'S' : 'N';
                pActivo = value;
            }
        }
        #endregion
        public char iva_situacion { get; set; }
        public decimal iva_alicuota { get; set; }
        public decimal in_alicuota { get; set; }
        public decimal P_Plista { get; set; }
        public decimal P_Dto1 { get; set; }
        public decimal P_Dto2 { get; set; }
        public decimal P_Dto3 { get; set; }
        public decimal P_Dto4 { get; set; }
        public decimal P_Dto_Pa { get; set; }
        public decimal P_Porc_Flete { get; set; }
        public string P_Boni { get; set; } = string.Empty;
        public decimal P_Pcosto { get; set; }
        public decimal lp_prevision_tot { get; set; }
        public decimal lp_prevision_pin { get; set; }
        public string lp_id { get; set; } = string.Empty;
        public decimal lp_margen { get; set; }
        public DateTime p_actu_fecha { get; set; }
        public string usu_id { get; set; } = string.Empty;
        public decimal p_neto { get; set; }
        public decimal p_iva { get; set; }
        public decimal p_in { get; set; }
        public decimal p_pvta { get; set; }
        public int carga { get; set; }
        public decimal tp_pista { get; set; }
        public decimal tp_dto1 { get; set; }
        public decimal tp_dto2 { get; set; }
        public decimal tp_dto3 { get; set; }
        public decimal tp_dto4 { get; set; }
        public decimal tp_dto_pa { get; set; }
        public decimal tp_porc_flete { get; set; }
        public int tp_boni { get; set; }
        public decimal tp_pcosto { get; set; }
        public decimal tin_alicuota { get; set; }
        public int lp_porc_mg { get; set; }
        public decimal tp_margen { get; set; }
        public decimal tp_pneto { get; set; }
        public decimal tp_iva { get; set; }
        public decimal tp_in { get; set; }
        public decimal tp_pvta { get; set; }


    }
}
