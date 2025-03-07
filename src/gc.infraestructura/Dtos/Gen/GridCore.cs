using X.PagedList;

namespace gc.infraestructura.Dtos.Gen
{
    public class GridCore<T>
    {
        public StaticPagedList<T>? ListaDatos { get; set; }
        public int CantidadReg { get; set; }
        public int PrimerRegistro { get; set; }
        public int UltimoRegistro { get; set; }
        public int RegistroFinal { get; set; }
        public int CantidadPaginas { get; set; }
        public int PaginaActual { get; set; }
        public string Sort { get; set; } = "Id";
        public string SortDir { get; set; } = "ASC";
        public string DatoAux01 { get; set; }=string.Empty;
    }

    public class GrillaCore<T, S>
    {
        public StaticPagedList<T>? ListaDatos { get; set; }
        public List<S>? ColumnasType { get; set; }
        public int CantidadReg { get; set; }
        public int PrimerRegistro { get; set; }
        public int UltimoRegistro { get; set; }
        public int RegistroFinal { get; set; }
        public int CantidadPaginas { get; set; }
        public int PaginaActual { get; set; }
        public string Sort { get; set; } = "Id";
        public string SortDir { get; set; } = "ASC";
    }
}
