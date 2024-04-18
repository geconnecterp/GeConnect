namespace gc.api.core.Entidades
{
    public partial class Usuarios : EntidadBase
    {
        public Usuarios()
        {
            usu_id = string.Empty;
            usu_password = string.Empty;
            usu_apellidoynombre = string.Empty;
            tdo_codigo = string.Empty;
            usu_ducumento = string.Empty;
            usu_email = string.Empty;
            usu_celu = string.Empty;
            usu_pin = string.Empty;
            cta_id = string.Empty;
        }

        public string usu_id { get; set; }
        public string usu_password { get; set; }
        public string usu_apellidoynombre { get; set; }
        public bool usu_bloqueado { get; set; }
        public DateTime? usu_bloqueado_fecha { get; set; }
        public short usu_intentos { get; set; }
        public bool usu_estalogeado { get; set; }
        public DateTime? usu_alta { get; set; }
        public bool? usu_expira { get; set; }
        public short? usu_dias_expiracion { get; set; }
        public DateTime? usu_fecha_expira_inicio { get; set; }
        public string tdo_codigo { get; set; }
        public string usu_ducumento { get; set; }
        public string usu_email { get; set; }
        public string usu_celu { get; set; }
        public string usu_pin { get; set; }
        public string cta_id { get; set; }


     

    }
}
