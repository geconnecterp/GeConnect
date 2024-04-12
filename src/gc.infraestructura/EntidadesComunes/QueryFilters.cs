using System;

namespace gc.infraestructura.Core.EntidadesComunes
{
    public class QueryFilters : BaseFilters
    {
        public QueryFilters() {
            Search = "";
        }
        public bool Todo { get { return (Id == default || Id == null) && IdRef == default && !Date.HasValue && string.IsNullOrWhiteSpace(Search) && PageSize == default && PageNumber == default; } }
        public object? Id { get; set; }
        public int IdRef { get; set; }
        public Guid IdG { get; set; }
        public DateTime? Date { get; set; }
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

    }
    public class BaseFilters
    {
        public string? Sort { get; set; }
        public string? SortDir { get; set; }
    }
}
