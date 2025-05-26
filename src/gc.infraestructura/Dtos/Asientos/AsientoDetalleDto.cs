namespace gc.infraestructura.Dtos.Asientos
{
    public class AsientoDetalleDto
    {
        public string Dia_movi { get; set; } = string.Empty;
        public DateTime Dia_fecha { get; set; }
        public string Dia_tipo { get; set; } = string.Empty;
        public string Dia_lista { get; set; } = string.Empty;
        public string Dia_desc_asiento { get; set; } = string.Empty;

        // Propiedades para los totales
        public decimal TotalDebe { get; set; }
        public decimal TotalHaber { get; set; }

        // Colección de líneas de detalle
        public List<AsientoLineaDto> Detalles { get; set; } = new List<AsientoLineaDto>();
    }

    public class AsientoLineaDto
    {
        public string Dia_movi { get; set; } = string.Empty;
        public int Dia_nro { get; set; }
        public string Ccb_id { get; set; } = string.Empty;
        public string Ccb_desc { get; set; } = string.Empty;
        public string Dia_desc { get; set; } = string.Empty;
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
    }

    public class AsientoPlanoDto
    {
        public string dia_movi { get; set; } = string.Empty;
        public DateTime dia_fecha { get; set; }
        public string dia_tipo { get; set; } = string.Empty;
        public string dia_lista { get; set; } = string.Empty;
        public string dia_desc_asiento { get; set; } = string.Empty;
        public int dia_nro { get; set; }
        public string ccb_id { get; set; } = string.Empty;
        public string ccb_desc { get; set; } = string.Empty;
        public string dia_desc { get; set; } = string.Empty;
        public decimal debe { get; set; }
        public decimal haber { get; set; }
    }

    public class AsientoAccionDto
    {
        public AsientoDetalleDto asiento { get; set; } = new();
        public char accion { get; set; }
    }
}
