
namespace gc.api.infra.Datos.Implementacion
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using gc.api.core.Interfaces.Datos;
    using gc.api.core.Entidades;
    using System.Data;
    using log4net.Core;
    using Microsoft.Extensions.Logging;
    using gc.api.infra.Datos.Contratos;
    using gc.infraestructura.Helpers;
    using Microsoft.Data.SqlClient;

    public class Repository<T> : IRepository<T> where T : EntidadBase
    {
        private readonly GeConnectContext _contexto;
        private readonly IDataConnectionContext _dbContext;
        public Repository(GeConnectContext contexto)
        {
            _contexto = contexto;
            _dbContext = new DataConnectionContext(contexto);            
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

       


        public List<T> EjecutarSP(string? sp, params object[] parametros)
        {
            StringBuilder sb = ProcesarParametros(sp, parametros);

            //return _contexto.Database.FromSql<T>(sb.ToString(), parametros).ToList();
            //definición de la ejecución de SP en ASP.NET Core
            return _contexto.Set<T>().FromSqlRaw<T>(sb.ToString(), parametros).ToList();
        }

        private static StringBuilder ProcesarParametros(string? sp, object[] parametros)
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

            return sb;
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

        public List<T> InvokarSp2Lst(string sp, List<SqlParameter> parametros,bool ignoreCase=false)
        {
            int contador = 0;
            List<T> resultado = null;

            using (var cnn = _dbContext.ObtenerConexionSql())
            {
                var cmd = _dbContext.ObtenerCommandSql(cnn, CommandType.StoredProcedure);
                cmd.CommandText = sp;
                foreach (var p in parametros)
                {
                    cmd.Parameters.Add(p);
                }
                cnn.Open();
                using (var dr = _dbContext.ObtenerDatosDelCommand(cmd))
                {
                    var mapper = new GenericDataMapper<T>();
                    resultado = new List<T>();
                    while (dr.Read())
                    {
                        contador++;
                        resultado.Add(mapper.Map(dr,ignoreCase));
                    }
                }
            }
            return resultado;
        }

        public int InvokarSpNQuery(string sp, List<SqlParameter> parametros, bool esTransacciona = false, bool elUltimo = true)
        {
            int resultado = 0;

            var cnn = _dbContext.ObtenerConexionSql(esTransacciona);

            var cmd = _dbContext.ObtenerCommandSql(cnn, CommandType.StoredProcedure);
            cmd.CommandText = sp;
            foreach (var p in parametros)
            {
                cmd.Parameters.Add(p);
            }
            //si es TRANSACCIONAL la conexion ya fue abierta al momento de generar la conexion y definir la transaccion para la operacion actual.
            if (!esTransacciona)
            {
                cnn.Open();
            }
            resultado = cmd.ExecuteNonQuery();
            if (esTransacciona && elUltimo)
            {
                _dbContext.GrabarCambios();
            }
            //en caso de ser el ultimo item de ejecución se procederá a cerrar la conexión
            if (elUltimo)
            {
                _dbContext.CerrarConexion();
            }

            return resultado;
        }

        public object InvokarSpScalar(string sp, List<SqlParameter>? parametros, bool esTransacciona = false, bool elUltimo = true, bool esSP = true)
        {
            object resultado = null;
            var cnn = _dbContext.ObtenerConexionSql(esTransacciona);

            SqlCommand cmd;
            if (esSP)
            {
                cmd = _dbContext.ObtenerCommandSql(cnn, CommandType.StoredProcedure);
            }
            else
            {
                cmd = _dbContext.ObtenerCommandSql(cnn, CommandType.Text);
            }

            cmd.CommandText = sp;

            if (parametros!=null)
            {
                foreach (var p in parametros)
                {
                    cmd.Parameters.Add(p);
                }
            }
            
            //si es TRANSACCIONAL la conexion ya fue abierta al momento de generar la conexion y definir la transaccion para la operacion actual.
            if (!esTransacciona)
            {
                cnn.Open();
            }
            resultado = cmd.ExecuteScalar();
            if (esTransacciona && elUltimo)
            {
                _dbContext.GrabarCambios();
            }
            //en caso de ser el ultimo item de ejecución se procederá a cerrar la conexión
            if (elUltimo)
            {
                _dbContext.CerrarConexion();
            }

            return resultado;
        }

        public List<T> InvokarSp2Lst(string sp)
        {
            return InvokarSp2Lst(sp, new List<SqlParameter>());
        }

        public List<T> InvokarSp2Lst(string sp, SqlParameter parametro)
        {
            return InvokarSp2Lst(sp, new List<SqlParameter> { parametro });
        }

        public int InvokarSpNQuery(string sp, SqlParameter parametro, bool esTransaccional = false, bool elUltimo = true)
        {
            return InvokarSpNQuery(sp, new List<SqlParameter> { parametro }, esTransaccional, elUltimo);
        }

        public object InvokarSpScalar(string sp, SqlParameter parametro, bool esTransacciona = false, bool elUltimo = true)
        {
            return InvokarSpScalar(sp, new List<SqlParameter> { parametro }, esTransacciona, elUltimo);
        }
    }
}
