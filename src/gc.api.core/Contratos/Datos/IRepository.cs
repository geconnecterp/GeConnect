namespace gc.api.core.Interfaces.Datos
{
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos.Almacen;
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
        SqlParameter[] InferirParametrosExt<S>(S entidad, IEnumerable<string>? excluir = null) where S : class;
        SqlParameter[] InferirParametros(T entidad, IEnumerable<string>? excluir = null);
        List<T> InvokarSp2Lst(string sp, List<SqlParameter> parametros,bool ignoreCase = false);
        int InvokarSpNQuery(string sp, List<SqlParameter> parametros, bool esTransacciona = false, bool elUltimo = true);
        object InvokarSpScalar(string sp, List<SqlParameter>? parametros, bool esTransacciona = false, bool elUltimo = true,bool esSP=true);
        List<S> EjecutarLstSpExt<S>(string sp, List<SqlParameter> ps, bool ignoreCase = false) where S : class;
        (List<S1> Primero, List<S2> Segundo) EjecutarSpDosResultados<S1, S2>(
            string sp,
            List<SqlParameter> parametros,
            bool ignoreCase = false)
            where S1 : class
            where S2 : class;
        List<TResult> EjecutarLstFunction<TResult>(string sqlFunction, List<SqlParameter> parameters = null, bool esTransaccion = false) where TResult : class, new();
        /// <summary>
        /// Ejecuta una función escalar que retorna un valor único
        /// </summary>
        TResult EjecutarFunctionScalar<TResult>(string sqlFunction, List<SqlParameter>? parameters = null, bool esTransaccion = false);

        /// <summary>
        /// Ejecuta una función escalar con un solo parámetro
        /// </summary>
        TResult EjecutarFunctionScalar<TResult>(string sqlFunction, SqlParameter parametro, bool esTransaccion = false);

        /// <summary>
        /// Ejecuta una función escalar sin parámetros
        /// </summary>
        TResult EjecutarFunctionScalar<TResult>(string sqlFunction, bool esTransaccion = false);

        /// <summary>
        /// Ejecuta una función escalar sin parámetros
        /// </summary>
        TResult EjecutarFunctionScalar<TResult>(string sqlFunction);
    }
}
