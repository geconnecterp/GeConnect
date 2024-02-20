
namespace gc.api.infra.Datos.Implementacion
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using gc.api.core.Interfaces.Datos;
    using gc.api.core.Entidades;

    public class Repository<T> : IRepository<T> where T : EntidadBase
    {
        private readonly GeConnectContext _contexto;

        public Repository(GeConnectContext contexto)
        {
            _contexto = contexto;
        }

        public T Find(object id)
        {
            return _contexto.Set<T>().Find(id);
        }

        public async Task<T> FindAsync(object id)
        {
            return await _contexto.Set<T>().FindAsync(id);
        }

        public IQueryable<T> GetAll()
        {
            return _contexto.Set<T>().Select(x => x);
        }

        public void Add(T entity)
        {
            _contexto.Set<T>().Add(entity);
        }

        public async Task AddAsync(T entity)
        {
            await _contexto.Set<T>().AddAsync(entity);
        }

        public void Update(T entity)
        {
            _contexto.Set<T>().Update(entity);
        }

        public void Remove(T entity)
        {
           if(_contexto.Entry(entity).State== Microsoft.EntityFrameworkCore.EntityState.Detached)
            {
                _contexto.Set<T>().Attach(entity);
            }
            _contexto.Set<T>().Remove(entity);
        }

        public List<T> EjecutarSP(string sp, params object[] parametros)
        {
            
            StringBuilder sb = new StringBuilder(sp + " ");
            bool first = true;

            foreach (SqlParameter p in parametros)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }
                sb.Append(p.ParameterName);
            }

            //return _contexto.Database.FromSql<T>(sb.ToString(), parametros).ToList();
            //definición de la ejecución de SP en ASP.NET Core
            return _contexto.Set<T>().FromSqlRaw<T>(sb.ToString(), parametros).ToList();
        }       

        public SqlParameter[] InferirParametros(T entidad, IEnumerable<string>? excluir = null)
        {
            List<SqlParameter> parametros = new List<SqlParameter>();
            if (excluir == null)
            {
                excluir = new List<string>();
            }

            var t = typeof(T);

            var propiedades = t.GetProperties().Where(p => !excluir.Contains(p.Name));

            foreach (var prop in propiedades)
            {
                var nn = "@" + prop.Name;
                var valor = prop.GetValue(entidad, null);

                parametros.Add(new SqlParameter(nn, valor));
            }

            return parametros.ToArray();
        }        
    }
}
