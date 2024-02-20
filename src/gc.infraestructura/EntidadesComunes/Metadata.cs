namespace gc.infraestructura.Core.EntidadesComunes
{
    public class Metadata
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TatalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }

        public string? NextPageUrl { get; set; }
        public string? PreviousPageUrl { get; set; }
    }
}
