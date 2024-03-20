namespace gc.infraestructura.Dtos
{
    public partial class AccesoDto:Dto
    {
        public AccesoDto()
        {
            UsuarioContrasena = string.Empty;
            IP = string.Empty;
        }
        public long Id { get; set; }
        public Guid UsuarioId { get; set; }
        public DateTime Fecha { get; set; }
        public string IP { get; set; }
        public char TipoAcceso { get; set; }

        public string UsuarioContrasena { get; set; }
    }
}
