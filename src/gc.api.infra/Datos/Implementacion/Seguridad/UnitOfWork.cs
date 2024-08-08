namespace gc.api.infra.Datos.Implementacion
{
    using gc.api.core.Entidades;
    using gc.api.core.Interfaces.Datos;
    using System;
    using System.Threading.Tasks;

    public class UnitOfWork : IUnitOfWork
    {
        public readonly GeConnectContext _contexto;

        public UnitOfWork(GeConnectContext contexto)
        {
            _contexto = contexto;
        }

        public IRepository<T> GetRepository<T>() where T : EntidadBase
        {
            IRepository<T> repository;
            repository = new Repository<T>(_contexto);
            return repository as IRepository<T>;
        }

        public int SaveChanges(bool process = true)
        {
            try
            {
                return _contexto.SaveChanges();
            }
            catch (Exception )
            {                
                throw;
            }
        }

        public async Task<int> SaveChangesAsync(bool process = true)
        {
            try
            {
                return await _contexto.SaveChangesAsync();
            }           
            catch (Exception)
            {             
                throw;
            }
        }
    }
}
