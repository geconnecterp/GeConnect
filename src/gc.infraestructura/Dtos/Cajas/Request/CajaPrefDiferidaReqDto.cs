namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class CajaPrefDiferidaReqDto
    {
        public string Caja_Id { get; set; } = string.Empty;
        public string Usu_Id { get; set; } = string.Empty;
        public string Adm_Id { get; set; } = string.Empty;
        public string Lp_Id { get; set; } = string.Empty;
        public string Caja_Nro_Proceso { get; set; } = string.Empty;
        public string? Caja_Nro_Cierre { get; set; }
        public string? Cta_Id { get; set; } = string.Empty;
        public string Tdoc_Id { get; set; } = string.Empty;
        public string? Cta_Documento { get; set; } = string.Empty;
        public string Cta_Denominacion { get; set; } = string.Empty;
        public string Sec_Id { get; set; } = string.Empty;
        public string Json_P { get; set; } = string.Empty;
    }
}
