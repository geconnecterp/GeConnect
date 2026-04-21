namespace gc.infraestructura.Dtos.Cajas
{
    public class CuentaBusquedaResultadoDto
    {
        public string Cta_Id { get; set; } = string.Empty;
        public string Cta_Denominacion { get; set; } = string.Empty;
        public string Cta_Domicilio { get; set; } = string.Empty;
        public string Cta_Celu { get; set; } = string.Empty;
        public string Cta_Email { get; set; } = string.Empty;
        public string Cta_Sexo { get; set; } = string.Empty;

        // Lo dejo como string por seguridad, porque no vemos el tipo real en la tabla.
        // Si en BD es int/smallint, puedes cambiarlo a int? o short?.
        public string Tdoc_Id { get; set; } = string.Empty;

        public string Tdoc_Desc { get; set; } = string.Empty;
        public string Cta_Documento { get; set; } = string.Empty;

        // Valores esperados: C, N, F
        public string Origen { get; set; } = string.Empty;

        public string Origen_Desc { get; set; } = string.Empty;
    }
}
