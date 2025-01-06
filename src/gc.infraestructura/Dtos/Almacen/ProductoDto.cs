using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Almacen
{
    public partial class ProductoDto : Dto
    {
        public ProductoDto()
        {
            p_id = string.Empty;
            p_m_marca = string.Empty;
            p_m_desc = string.Empty;
            p_m_capacidad = string.Empty;
            p_id_prov = string.Empty;
            p_desc = string.Empty;
            up_id = string.Empty;
            rub_id = string.Empty;
            cta_id = string.Empty;
            pg_id = string.Empty;
            //usu_id_alta = string.Empty;
            //usu_id_modi = string.Empty;
            p_obs = string.Empty;
            p_balanza_id = string.Empty;
        }
        [Display(Name = "Id")]
        public string p_id { get; set; }//
        [Display(Name = "Marca")]
        public string p_m_marca { get; set; }//
        [Display(Name = "Descripción")]
        public string p_m_desc { get; set; }//
        [Display(Name = "Capacidad")]
        public string p_m_capacidad { get; set; }//
        [Display(Name = "Prov.Id")]
        public string p_id_prov { get; set; }//
        [Display(Name = "Descripción")]
        public string p_desc { get; set; }//
        /*----------------------------------------------*/
        public char? p_alta_rotacion { get; set; }//
        private bool pAltaRotacion { get; set; }//
        public bool PAltaRotacion
        {
            get
            {
                if (!p_alta_rotacion.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(p_alta_rotacion.Value)))
                    return false;
                return p_alta_rotacion.Equals('S');
            }
            set
            {
                pAltaRotacion = value;
            }
        }
        /*----------------------------------------------*/
        public char? p_con_vto { get; set; }//
        private bool pConVto { get; set; }//
        public bool PConVto
        {
            get
            {
                if (!p_con_vto.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(p_con_vto.Value)))
                    return false;
                return p_con_vto.Equals('S');
            }
            set
            {
                pConVto = value;
            }
        }
        public short p_con_vto_min { get; set; }
        public decimal? p_peso { get; set; }//
        /*----------------------------------------------*/
        public char p_elaboracion { get; set; }//
        private bool pElaboracion { get; set; }//
        public bool PElaboracion
        {
            get
            {
                if (char.IsWhiteSpace(p_elaboracion) ||
                    string.IsNullOrWhiteSpace(char.ToString(p_elaboracion)))
                    return false;
                return p_elaboracion.Equals('S');
            }
            set
            {
                //P_Elaboracion = value ? 'S' : 'N';
                pElaboracion = value;
            }
        }
        /*----------------------------------------------*/
        public char p_materia_prima { get; set; }//
        private bool pMatPri;
        public bool PMatPri
        {
            get
            {
                if (char.IsWhiteSpace(p_materia_prima) || string.IsNullOrWhiteSpace(char.ToString(p_materia_prima)))
                    return false;
                return p_materia_prima == 'S';
            }
            set {
                //P_Materia_Prima = value ? 'S' : 'N';
                pMatPri = value; }
        }
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
        public string pg_desc { get; set; }
        public string pg_lista { get; set; }
        //public short? P_Orden_Pg { get; set; }
        //public decimal P_Plista { get; set; }
        //public decimal P_Dto1 { get; set; }
        //public decimal P_Dto2 { get; set; }
        //public decimal P_Dto3 { get; set; }
        //public decimal P_Dto4 { get; set; }
        //public decimal P_Dto5 { get; set; }
        //public decimal P_Dto6 { get; set; }
        //public decimal P_Dto_Pa { get; set; }
        //public string P_Boni { get; set; }
        //public decimal P_Porc_Flete { get; set; }
        public decimal in_alicuota { get; set; }
        public decimal iva_alicuota { get; set; }
        public char iva_situacion { get; set; }
        //public decimal P_Pcosto { get; set; }
        //public decimal P_Pcosto_Repo { get; set; }
        //public DateTime? p_alta { get; set; }
        //public string usu_id_alta { get; set; }
        //public DateTime? p_modi { get; set; }
        //public string usu_id_modi { get; set; }
        public char? p_actu { get; set; }
        public string p_obs { get; set; }
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
        public char? p_balanza { get; set; }
        private bool pBalaza;
        public bool PBalanza
        {
            get
            {
                if (!p_balanza.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(p_balanza.Value)))
                    return false;
                return p_balanza.Equals('S');
            }
            set
            {
                pBalaza = value;
            }
        }
        public short? p_balanza_dvto { get; set; }
        public string p_balanza_id { get; set; }
        public char? adm_min_excluye { get; set; }
        private bool admMinExcluye;
        public bool AdmMinExcluye
        {
            get
            {
                if (!adm_min_excluye.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(adm_min_excluye.Value)))
                    return false;
                return adm_min_excluye.Equals('S');
            }
            set
            {
                admMinExcluye = value;
            }
        }
        public char? adm_may_excluye { get; set; }
        private bool admMayExcluye;
        public bool AdmMayExcluye
        {
            get
            {
                if (!adm_may_excluye.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(adm_may_excluye.Value)))
                    return false;
                return adm_may_excluye.Equals('S');
            }
            set
            {
                admMayExcluye = value;
            }
        }
        public char? pi_auto_excluye { get; set; }
        private bool piAutoExluye;
        public bool PiAutoExluye
        {
            get
            {
                if (!pi_auto_excluye.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(pi_auto_excluye.Value)))
                    return false;
                return pi_auto_excluye.Equals('S');
            }
            set
            {
                piAutoExluye = value;
            }
        }
        public char? oc_auto_excluye { get; set; }
        private bool ocAutoExluye;
        public bool OcAutoExluye
        {
            get
            {
                if (!oc_auto_excluye.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(oc_auto_excluye.Value)))
                    return false;
                return oc_auto_excluye.Equals('S');
            }
            set
            {
                ocAutoExluye = value;
            }
        }

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
