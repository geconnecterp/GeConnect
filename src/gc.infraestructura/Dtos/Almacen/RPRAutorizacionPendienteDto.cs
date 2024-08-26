namespace gc.infraestructura.Dtos.Almacen
{
    public class RPRAutorizacionPendienteDto:Dto
    {
        public string Rp { get; set; }=string.Empty;
        public string Cta_id { get; set; } = string.Empty;
        public string Usu_id { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Nota { get; set; } = string.Empty;
        public string Rpe_id { get; set; } = string.Empty;
        public string Cta_denominacion { get; set; } = string.Empty;
    }

    public class RPRRegistroResponseDto
    {
        public short Resultado { get; set; }
        public string Resultado_msj { get; set; }=string.Empty;   
    }
}
