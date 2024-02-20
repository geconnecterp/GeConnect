namespace gc.api.core.Interfaces.Datos
{
    using System.Threading.Tasks;
    using gc.api.core.Entidades;

    public interface IUnitOfWork
    {
        IRepository<T> GetRepository<T>() where T : EntidadBase;        
        int SaveChanges(bool process = true);
        Task<int> SaveChangesAsync(bool process = true);
    }
}
