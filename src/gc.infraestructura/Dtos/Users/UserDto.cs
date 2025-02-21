namespace gc.infraestructura.Dtos.Users
{
    public class UserDto : Dto
    {
        public int Total_registros { get; set; } = 0;
        public int Total_paginas { get; set; } = 0;
        public string usu_id { get; set; } = string.Empty;
        public string usu_apellidoynombre { get; set; } = string.Empty;
        public bool usu_bloqueado { get; set; } = false;
        public string tdoc_id { get; set; } = string.Empty;
        public string tdoc_desc { get; set; } = string.Empty;
        public string usu_documento { get; set; } = string.Empty;
        public string? usu_celu { get; set; } = string.Empty;
        public string? usu_email { get; set; } = string.Empty;
        public string? cta_id { get; set; } = string.Empty;
        public string? cta_denominacion { get; set; } = string.Empty;
        public string? usu_pin { get; set; } = string.Empty;
    }
}
