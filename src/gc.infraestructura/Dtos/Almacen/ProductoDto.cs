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
            Lp_Id_Default = string.Empty;
        }
        [Display(Name = "Id")]
        public string P_Id { get; set; }//
        [Display(Name = "Marca")]
        public string P_M_Marca { get; set; }//
        [Display(Name = "Descripción")]
        public string P_M_Desc { get; set; }//
        [Display(Name = "Capacidad")]
        public string P_M_Capacidad { get; set; }//
        [Display(Name ="Prov.Id")]
        public string P_Id_Prov { get; set; }//
        [Display(Name ="Descripción")]
        public string P_Desc { get; set; }//
        public char? P_Alta_Rotacion { get; set; }//
        public char? P_Con_Vto { get; set; }//
        public  short P_Con_Vto_Min { get; set; }
        public decimal? P_Peso { get; set; }//
        public char P_Elaboracion { get; set; }//
        public char P_Materia_Prima { get; set; }//
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
        public char? P_Balanza { get; set; }
        public short? P_Balanza_Dvto { get; set; }
        public string P_Balanza_Id { get; set; }
        public char Adm_Min_Excluye { get; set; }
        public char Adm_May_Excluye { get; set; }
        public char Pi_Auto_Excluye { get; set; }
        public string Lp_Id_Default { get; set; }
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
