using gc.infraestructura.Core.EntidadesComunes;
namespace gc.infraestructura.Dtos.Asientos
{
    public class QueryAsiento : BaseFilters
    {
        public int Eje_nro { get; set; }
        public bool Movi { get; set; } = true;
        public string Movi_like { get; set; } = string.Empty;
        public bool Usu { get; set; }
        public string Usu_like { get; set; } = string.Empty;
        public bool Tipo { get; set; } = true;
        public string Tipo_like { get; set; } = string.Empty;
        public string Ccb_id { get; set; } = string.Empty;
        public bool Rango { get; set; } = true;
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public bool EsTemporal { get; set; }
    }
}
