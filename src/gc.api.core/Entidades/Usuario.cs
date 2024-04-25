namespace gc.api.core.Entidades
{
    public partial class Usuario : EntidadBase
    {
        public Usuario()
        {
            Usu_id = string.Empty;
            Usu_password = string.Empty;
            Usu_apellidoynombre = string.Empty;
            Tdo_codigo = string.Empty;
            Usu_ducumento = string.Empty;
            Usu_email = string.Empty;
            Usu_celu = string.Empty;
            Usu_pin = string.Empty;
            Cta_id = string.Empty;
        }

        public string Usu_id { get; set; }
        public string Usu_password { get; set; }
        public string Usu_apellidoynombre { get; set; }
        public bool Usu_bloqueado { get; set; }
        public DateTime? Usu_bloqueado_fecha { get; set; }
        public short Usu_intentos { get; set; }
        public bool Usu_estalogeado { get; set; }
        public DateTime? Usu_alta { get; set; }
        public bool? Usu_expira { get; set; }
        public short? Usu_dias_expiracion { get; set; }
        public DateTime? Usu_fecha_expira_inicio { get; set; }
        public string Tdo_codigo { get; set; }
        public string Usu_ducumento { get; set; }
        public string Usu_email { get; set; }
        public string Usu_celu { get; set; }
        public string Usu_pin { get; set; }
        public string Cta_id { get; set; }


     

    }
}
