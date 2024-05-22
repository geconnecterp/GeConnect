namespace gc.api.core.Interfaces.Datos
{
    using gc.api.core.Entidades;
    using Microsoft.Data.SqlClient;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;


    public interface IRepository<T> where T : EntidadBase
    {
        T Find(object id);
        Task<T> FindAsync(object id);
        IQueryable<T> GetAll();
        void Add(T entity);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        List<T> EjecutarSP(string?sp, params object[] parametros);     
        SqlParameter[] InferirParametros(T entidad, IEnumerable<string>? excluir = null);
        List<T> InvokarSp2Lst(string sp, List<SqlParameter> parametros,bool ignoreCase = false);
        int InvokarSpNQuery(string sp, List<SqlParameter> parametros, bool esTransacciona = false, bool elUltimo = true);
        object InvokarSpScalar(string sp, List<SqlParameter>? parametros, bool esTransacciona = false, bool elUltimo = true,bool esSP=true);

    }
}
