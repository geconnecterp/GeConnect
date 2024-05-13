namespace gc.api.core.Entidades
{
    public partial class BilleteraOrden : EntidadBase
    {
        public BilleteraOrden()
        {
            Bo_Id = string.Empty;
            Rb_Compte = string.Empty;//
            Adm_Id = string.Empty;//
            Caja_Id = string.Empty;//
            Bill_Id = string.Empty;//
            Boe_Id = string.Empty;
            Cuit = string.Empty;//
            Tco_Id = string.Empty;//
            Cm_Compte = string.Empty;//
            Bo_Clave = string.Empty;
            Bo_Id_Ext = string.Empty;
            Bo_Notificado_Desc = string.Empty;
            Ip = string.Empty;
        }

        public string Bo_Id { get; set; }
        public string Rb_Compte { get; set; }//
        public string Adm_Id { get; set; }//
        public string Caja_Id { get; set; }//
        public string Bill_Id { get; set; }//
        public string Boe_Id { get; set; }
        public string Cuit { get; set; }//
        public string Tco_Id { get; set; }//
        public string Cm_Compte { get; set; }//
        public decimal Bo_Importe { get; set; }//
        public DateTime? Bo_Carga { get; set; }
        public string? Bo_Clave { get; set; }//
        public string? Bo_Id_Ext { get; set; }
        public DateTime? Bo_Notificado { get; set; }
        public string? Bo_Notificado_Desc { get; set; }
        public string Ip { get; set; }//



    }
}
