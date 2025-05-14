namespace gc.infraestructura.Dtos.Asientos
{
    public class EjercicioDto:Dto
    {
        public string Eje_nro { get; set; } = string.Empty; 
        public DateTime Eje_desde { get; set; }
        public DateTime Eje_hasta { get; set; }
        public char Eje_activo { get; set; }
        public string Usu_id_abre { get; set; } = string.Empty;
        public string usu_id_cierra { get; set; } = string.Empty;
        public DateTime Eje_ctl { get; set; }
        public string Eje_lista { get; set; } = string.Empty;
    }
}
