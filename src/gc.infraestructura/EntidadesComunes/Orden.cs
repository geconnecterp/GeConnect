namespace gc.infraestructura.EntidadesComunes
{
    public class Orden
    {
        public string Sort { get; set; } =string.Empty;
        public SortDirection SortDir { get; set; }= SortDirection.Asc;
    }

    public enum SortDirection { 
        Asc,
        Desc
    }
}
