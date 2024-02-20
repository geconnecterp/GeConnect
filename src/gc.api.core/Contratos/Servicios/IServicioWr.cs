namespace gc.api.core.Interfaces.Servicios
{
    using gc.api.core.Entidades;
    using System.Threading.Tasks;

    public interface IServicioWr<T> where T : EntidadBase
    {
       
        void Add(T item);
        Task<bool> AddAsync(T item);
        Task<bool> Update(T item);
        Task<bool> Delete(object id);
    }
}
