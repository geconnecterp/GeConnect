namespace gc.infraestructura.Dtos.Productos
{
    public class InfoProductoDto:Dto
    {
        public string Id { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Descipcion { get; set; } = string.Empty;
        public string Capacidad { get; set; } = string.Empty;
        public string ProveedorId { get; set; } = string.Empty;
        public string Familia { get; set; } = string.Empty;
        public string Rubro { get; set; } = string.Empty;
    }
}
