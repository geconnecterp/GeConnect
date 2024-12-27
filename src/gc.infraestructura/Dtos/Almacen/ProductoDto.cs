using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Almacen
{
    public partial class ProductoDto : Dto
    {
        public ProductoDto()
        {
            P_Id = string.Empty;
            P_M_Marca = string.Empty;
            P_M_Desc = string.Empty;
            P_M_Capacidad = string.Empty;
            P_Id_Prov = string.Empty;
            P_Desc = string.Empty;
            Up_Id = string.Empty;
            Rub_Id = string.Empty;
            Cta_Id = string.Empty;
            Pg_Id = string.Empty;
            Usu_Id_Alta = string.Empty;
            Usu_Id_Modi = string.Empty;
            P_Obs = string.Empty;
            P_Balanza_Id = string.Empty;
        }
        [Display(Name = "Id")]
        public string P_Id { get; set; }//
        [Display(Name = "Marca")]
        public string P_M_Marca { get; set; }//
        [Display(Name = "Descripción")]
        public string P_M_Desc { get; set; }//
        [Display(Name = "Capacidad")]
        public string P_M_Capacidad { get; set; }//
        [Display(Name = "Prov.Id")]
        public string P_Id_Prov { get; set; }//
        [Display(Name = "Descripción")]
        public string P_Desc { get; set; }//
        /*----------------------------------------------*/
        public char? P_Alta_Rotacion { get; set; }//
        private bool pAltaRotacion { get; set; }//
        public bool PAltaRotacion
        {
            get
            {
                if (!P_Alta_Rotacion.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(P_Alta_Rotacion.Value)))
                    return false;
                return P_Alta_Rotacion.Equals('S');
            }
            set
            {
                pAltaRotacion = value;
            }
        }
        /*----------------------------------------------*/
        public char? P_Con_Vto { get; set; }//
        private bool pConVto { get; set; }//
        public bool PConVto
        {
            get
            {
                if (!P_Con_Vto.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(P_Con_Vto.Value)))
                    return false;
                return P_Con_Vto.Equals('S');
            }
            set
            {
                pConVto = value;
            }
        }
        public short P_Con_Vto_Min { get; set; }
        public decimal? P_Peso { get; set; }//
        /*----------------------------------------------*/
        public char P_Elaboracion { get; set; }//
        private bool pElaboracion { get; set; }//
        public bool PElaboracion
        {
            get
            {
                if (char.IsWhiteSpace(P_Elaboracion) ||
                    string.IsNullOrWhiteSpace(char.ToString(P_Elaboracion)))
                    return false;
                return P_Elaboracion.Equals('S');
            }
            set
            {
                //P_Elaboracion = value ? 'S' : 'N';
                pElaboracion = value;
            }
        }
        /*----------------------------------------------*/
        public char P_Materia_Prima { get; set; }//
        private bool pMatPri;
        public bool PMatPri
        {
            get
            {
                if (char.IsWhiteSpace(P_Materia_Prima) || string.IsNullOrWhiteSpace(char.ToString(P_Materia_Prima)))
                    return false;
                return P_Materia_Prima == 'S';
            }
            set {
                //P_Materia_Prima = value ? 'S' : 'N';
                pMatPri = value; }
        }
        public string Up_Id { get; set; }//
        public string Up_Desc { get; set; }
        public string Up_List { get; set; }
        public string Rub_Id { get; set; }
        public string Rub_Desc { get; set; }
        public string Rub_Lista { get; set; }
        public string Cta_Id { get; set; }
        public string Cta_Denominacion { get; set; }
        public string Cta_Lista { get; set; }
        public string Pg_Id { get; set; }
        public string Pg_Desc { get; set; }
        public string Pg_Lista { get; set; }
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
        public decimal In_Alicuota { get; set; }
        public decimal Iva_Alicuota { get; set; }
        public char Iva_Situacion { get; set; }
        //public decimal P_Pcosto { get; set; }
        //public decimal P_Pcosto_Repo { get; set; }
        public DateTime? P_Alta { get; set; }
        public string Usu_Id_Alta { get; set; }
        public DateTime? P_Modi { get; set; }
        public string Usu_Id_Modi { get; set; }
        public char? P_Actu { get; set; }
        public string P_Obs { get; set; }
        public char P_Activo { get; set; }
        private bool pActivo;
        public bool PActivo
        {
            get
            {
                if (char.IsWhiteSpace(P_Activo) || 
                    string.IsNullOrWhiteSpace(char.ToString(P_Activo)))
                    return false;
                return P_Activo == 'S';
            }
            set
            {
                //P_Materia_Prima = value ? 'S' : 'N';
                pActivo = value;
            }
        }
        public char? P_Balanza { get; set; }
        private bool pBalaza;
        public bool PBalanza
        {
            get
            {
                if (!P_Balanza.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(P_Balanza.Value)))
                    return false;
                return P_Balanza.Equals('S');
            }
            set
            {
                pBalaza = value;
            }
        }
        public short? P_Balanza_Dvto { get; set; }
        public string P_Balanza_Id { get; set; }
        public char? Adm_Min_Excluye { get; set; }
        private bool admMinExcluye;
        public bool AdmMinExcluye
        {
            get
            {
                if (!Adm_Min_Excluye.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(Adm_Min_Excluye.Value)))
                    return false;
                return Adm_Min_Excluye.Equals('S');
            }
            set
            {
                admMinExcluye = value;
            }
        }
        public char? Adm_May_Excluye { get; set; }
        private bool admMayExcluye;
        public bool AdmMayExcluye
        {
            get
            {
                if (!Adm_May_Excluye.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(Adm_May_Excluye.Value)))
                    return false;
                return Adm_May_Excluye.Equals('S');
            }
            set
            {
                admMayExcluye = value;
            }
        }
        public char? Pi_Auto_Excluye { get; set; }
        private bool piAutoExluye;
        public bool PiAutoExluye
        {
            get
            {
                if (!Pi_Auto_Excluye.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(Pi_Auto_Excluye.Value)))
                    return false;
                return Pi_Auto_Excluye.Equals('S');
            }
            set
            {
                piAutoExluye = value;
            }
        }
        public char? Oc_Auto_Excluye { get; set; }
        private bool ocAutoExluye;
        public bool OcAutoExluye
        {
            get
            {
                if (!Oc_Auto_Excluye.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(Oc_Auto_Excluye.Value)))
                    return false;
                return Oc_Auto_Excluye.Equals('S');
            }
            set
            {
                ocAutoExluye = value;
            }
        }

        public char? Lp_Id_Default { get; set; }
        private bool lpIdDefault;
        public bool LpIdDefault
        {
            get
            {
                if (!Lp_Id_Default.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(Lp_Id_Default.Value)))
                    return false;
                return Lp_Id_Default.Equals('S');
            }
            set
            {
                lpIdDefault = value;
            }
        }
        public string P_Id_Barrado_Ean { get; set; }
        public int P_Unidad_Pres_Ean { get; set; }
        public int P_Unidad_X_Bulto_Ean { get; set; }
        public int P_Bulto_X_Piso_Ean { get; set; }
        public int P_Piso_X_Pallet_Ean { get; set; }
        public string P_Id_Barrado_Dun { get; set; }
        public int P_Unidad_Pres_Dun { get; set; }
        public int P_Unidad_X_Bulto_Dun { get; set; }
        public int P_Bulto_X_Piso_Dun { get; set; }
        public int P_Piso_X_Pallet_Dun { get; set; }

    }
}
