using X.PagedList;

namespace gc.sitio.Models.ViewModels
{
    public class GridCore<T>
    {
        public StaticPagedList<T> ListaDatos { get; set; }
        public int CantidadReg { get; set; }
        public int PrimerRegistro { get; set; }
        public int UltimoRegistro { get; set; }
        public int RegistroFinal { get; set; }
        public int CantidadPaginas { get; set; }
        public int PaginaActual { get; set; }
        public string Sort { get; set; }
        public string SortDir { get; set; }
    }
}
