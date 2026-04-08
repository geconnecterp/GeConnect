namespace gc.infraestructura.Dtos.Cajas
{
    public class CuentaBusquedaResultadoDto
    {
        public string CtaId { get; set; } = string.Empty;
        public string CtaDenominacion { get; set; } = string.Empty;
        public string CtaDomicilio { get; set; } = string.Empty;
        public string CtaCelu { get; set; } = string.Empty;
        public string CtaEmail { get; set; } = string.Empty;

        // Lo dejo como string por seguridad, porque no vemos el tipo real en la tabla.
        // Si en BD es int/smallint, puedes cambiarlo a int? o short?.
        public string TdocId { get; set; } = string.Empty;

        public string TdocDesc { get; set; } = string.Empty;
        public string CtaDocumento { get; set; } = string.Empty;

        // Valores esperados: C, N, F
        public string Origen { get; set; } = string.Empty;

        public string OrigenDesc { get; set; } = string.Empty;
    }
}
