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
        public string Estado { get; set; }= string.Empty;   
        public string Estado_Msj { get; set; }=string.Empty;   
    }

    public class RPRAutoComptesPendientesDto : RPRAutorizacionPendienteDto
    {
        public string Rpe_desc { get; set; }=string.Empty ;
        public char Tco_id { get; set; } = char.MinValue;
        public string Cm_compte { get; set; } = string.Empty;
        public DateTime Cm_fecha { get; set; }
        public decimal Cm_importe { get; set; } = decimal.Zero;
    }
}
