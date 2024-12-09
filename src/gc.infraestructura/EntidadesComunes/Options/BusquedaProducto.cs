namespace gc.infraestructura.EntidadesComunes.Options
{
    public class BusquedaBase
    {
        public string Busqueda { get; set; } = string.Empty;
        public string? ListaPrecio { get; set; } = string.Empty;
        public string Administracion { get; set; } = string.Empty;
        public string? TipoOperacion { get; set; } = string.Empty;
        public decimal DescuentoCli { get; set; }
    }

    public class BusquedaProducto : BusquedaBase
    {
        public BusquedaProducto()
        {
        }

        public string CtaProveedorId { get; set; } = string.Empty;
        public bool CtaProveedorIdUnico { get; set; }
        public string RubroId { get; set; } = string.Empty;
        public bool RubroIdUnico { get; set; }
        public bool OpcionEstado { get; set; }
        public bool EstadoActivo { get; set; }
        public bool EstadoDiscont { get; set; }
        public bool EstadoInactivo { get; set; }
        public bool OpcionStock { get; set; }
        public bool ConStock { get; set; }
        public bool SinStock { get; set; }
        public bool AbrirAppBusqueda { get; set; }
        public int? Registros { get; set; }
        public int? Pagina { get; set; }
        public string Sort { get; set; }
        public string SortDir { get; set; }
    }
}
