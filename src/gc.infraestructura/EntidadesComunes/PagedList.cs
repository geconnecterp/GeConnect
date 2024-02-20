

namespace gc.infraestructura.Core.EntidadesComunes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int? NextPageNumber => HasNextPage ? CurrentPage + 1 : new int?();
        public int? PreviousPageNumber => HasPreviousPage ? CurrentPage - 1 : new int?();

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(count / (decimal)pageSize) : 0;

            AddRange(items); //agregar todos los items que van a ser parte de la lista. Las propiedades son propiedades adicionales generadas.
        }

        public static PagedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            List<T> items;
            if (count > 0)
            {
                var datos = source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                items = datos.ToList();
            }
            else
            {
                items = new List<T>();
            }

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        public static PagedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
