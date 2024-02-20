namespace gc.infraestructura.Dtos
{
    public class EmpleadoDto
    {
        public Guid Id { get; set; }
        public int CodigoInterno { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public int ProvinciaId { get; set; }
        public string? ProvinciaNombre { get; set; }
        public int LocalidadId { get; set; }
        public string? LocalidadNombre { get; set; }
        public int TipoDocumentoId { get; set; }
        public string? TipoDocumentoNombre { get; set; }
        public string? CUIT { get; set; }
        public string? Email { get; set; }
        public Guid? UsuarioId { get; set; }
        public string? Usuario { get; set; }
        public string? Rol { get; set; }
        public bool? Activo { get; set; }

    }
}
