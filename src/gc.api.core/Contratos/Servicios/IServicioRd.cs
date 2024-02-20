namespace gc.api.core.Interfaces.Servicios
{
    using gc.api.core.Entidades;
    using gc.infraestructura.Core.EntidadesComunes;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IServicioRd<T> where T : EntidadBase
    {
        T Find(object id);
        Task<T> FindAsync(object id);

        IQueryable<T> GetAllIq();
        PagedList<T> GetAll(QueryFilters filters);
       
    }
}
