namespace gc.api.infra.Datos.Implementacion
{
    using gc.api.core.Entidades;
    using gc.api.core.Interfaces.Datos;
    using gc.api.core.Servicios.Reportes;
    using gc.api.infra.Datos.Contratos;
    using System;
    using System.Threading.Tasks;

    public class UnitOfWork : IUnitOfWork
    {
        //public readonly GeConnectContext _contexto;
        public readonly IDataConnectionContext _contexto;

        public UnitOfWork(IDataConnectionContext contexto)
        {
            _contexto = contexto;
        }

        public void Commit()
        {
           _contexto.Commit();
        }

        public long Complete()
        {
            return 1;
        }

        public IRepository<T> GetRepository<T>() where T : EntidadBase
        {
            return new Repository<T>(_contexto);
        }

        public void InicializarTransaccion()
        {
            _contexto.InicializarTransaccion();
        }

        public void Rollback()
        {
            _contexto.Rollback();
        }

        public int SaveChanges(bool process = true)
        {
            try
            {
                return _contexto.ObtenerDbContext().SaveChanges();
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
                return await _contexto.ObtenerDbContext().SaveChangesAsync();
            }           
            catch (Exception)
            {             
                throw;
            }
        }
    }
}
