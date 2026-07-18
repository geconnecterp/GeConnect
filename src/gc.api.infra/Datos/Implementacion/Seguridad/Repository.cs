
namespace gc.api.infra.Datos.Implementacion
{
    using gc.api.core.Entidades;
    using gc.api.core.Interfaces.Datos;
    using gc.api.infra.Datos.Contratos;
    using gc.infraestructura.Helpers;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public class Repository<T> : IRepository<T> where T : EntidadBase
    {
        private readonly GeConnectContext _contexto;
        private readonly IDataConnectionContext _dbContext;
        public Repository(GeConnectContext contexto)
        {
            _contexto = contexto;
            _dbContext = new DataConnectionContext(contexto);
        }

        internal Repository(IDataConnectionContext dataConnectionContext)
        {
            _dbContext = dataConnectionContext;
            _contexto = dataConnectionContext.ObtenerDbContext();
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
            if (_contexto.Entry(entity).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
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

        public SqlParameter[] InferirParametrosExt<S>(S entidad, IEnumerable<string>? excluir = null) where S : class
        {
            List<SqlParameter> parametros = InferirParametrosGen(entidad, excluir);

            return parametros.ToArray();
        }
        public SqlParameter[] InferirParametros(T entidad, IEnumerable<string>? excluir = null)
        {
            List<SqlParameter> parametros = InferirParametrosGen(entidad, excluir);

            return parametros.ToArray();
        }

        private static List<SqlParameter> InferirParametrosGen<S>(S entidad, IEnumerable<string>? excluir) where S : class
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

            return parametros;
        }

        public List<S> EjecutarLstSpExt<S>(string sp, List<SqlParameter> parametros, bool ignoreCase = false) where S : class
        {
            int contador = 0;
            List<S> resultado;

            using (var cnn = _dbContext.ObtenerConexionSql())
            {
                var cmd = _dbContext.ObtenerCommandSql(cnn, CommandType.StoredProcedure);
                cmd.CommandText = sp;
                cmd.CommandTimeout = 600;
                foreach (var p in parametros)
                {
                    cmd.Parameters.Add(p);
                }
                cnn.Open();
                using (var dr = _dbContext.ObtenerDatosDelCommand(cmd))
                {
                    var mapper = new GenericDataMapper<S>();
                    resultado = [];
                    while (dr.Read())
                    {
                        contador++;
                        resultado.Add(mapper.Map(dr, ignoreCase));
                    }
                }
            }
            return resultado;
        }

        public (List<S1> Primero, List<S2> Segundo) EjecutarSpDosResultados<S1, S2>(
            string sp,
            List<SqlParameter> parametros,
            bool ignoreCase = false)
            where S1 : class
            where S2 : class
        {
            var primero = new List<S1>();
            var segundo = new List<S2>();

            using var cnn = _dbContext.ObtenerConexionSql();
            using var cmd = _dbContext.ObtenerCommandSql(cnn, CommandType.StoredProcedure);
            cmd.CommandText = sp;
            cmd.CommandTimeout = 600;

            foreach (var parametro in parametros)
            {
                cmd.Parameters.Add(parametro);
            }

            cnn.Open();
            using var reader = _dbContext.ObtenerDatosDelCommand(cmd);
            var mapperPrimero = new GenericDataMapper<S1>();
            while (reader.Read())
            {
                primero.Add(mapperPrimero.Map(reader, ignoreCase));
            }

            if (reader.NextResult())
            {
                var mapperSegundo = new GenericDataMapper<S2>();
                while (reader.Read())
                {
                    segundo.Add(mapperSegundo.Map(reader, ignoreCase));
                }
            }

            return (primero, segundo);
        }

        public List<T> InvokarSp2Lst(string sp, List<SqlParameter> parametros, bool ignoreCase = false)
        {
            int contador = 0;
            List<T> resultado;

            using (var cnn = _dbContext.ObtenerConexionSql())
            {
                var cmd = _dbContext.ObtenerCommandSql(cnn, CommandType.StoredProcedure);
                cmd.CommandText = sp;
                cmd.CommandTimeout = 600;
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
                        resultado.Add(mapper.Map(dr, ignoreCase));
                    }
                }
            }
            return resultado;
        }

        /// <summary>
        /// Ejecuta una función escalar con un solo parámetro
        /// </summary>
        /// <typeparam name="TResult">Tipo de resultado esperado</typeparam>
        /// <param name="sqlFunction">Consulta SQL</param>
        /// <param name="parametro">Parámetro único</param>
        /// <param name="esTransaccion">Indica si se ejecuta dentro de una transacción</param>
        /// <returns>Valor único del tipo especificado</returns>
        public TResult EjecutarFunctionScalar<TResult>(string sqlFunction,
            SqlParameter parametro, bool esTransaccion = false)
        {
            return EjecutarFunctionScalar<TResult>(sqlFunction, new List<SqlParameter> { parametro }, esTransaccion);
        }

        /// <summary>
        /// Ejecuta una función escalar sin parámetros
        /// </summary>
        /// <typeparam name="TResult">Tipo de resultado esperado</typeparam>
        /// <param name="sqlFunction">Consulta SQL</param>
        /// <param name="esTransaccion">Indica si se ejecuta dentro de una transacción</param>
        /// <returns>Valor único del tipo especificado</returns>
        public TResult EjecutarFunctionScalar<TResult>(string sqlFunction, bool esTransaccion = false)
        {
            return EjecutarFunctionScalar<TResult>(sqlFunction, (List<SqlParameter>?)null, esTransaccion);
        }

        public TResult EjecutarFunctionScalar<TResult>(string sqlFunction)
        {
            return EjecutarFunctionScalar<TResult>(sqlFunction, (List<SqlParameter>?)null, false);
        }

        public List<TResult> EjecutarLstFunction<TResult>(string sqlFunction, List<SqlParameter> parameters = null, bool esTransaccion = false) where TResult : class, new()
        {
            try
            {
                using var connection = _dbContext.ObtenerConexionSql();
                connection.Open();

                SqlCommand command = new(sqlFunction, connection);
                command.CommandType = CommandType.Text;  // Importante: CommandType.Text porque es una función

                // Agregar parámetros si existen
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }
                }

                List<TResult> resultado = new();

                // Ejecutar la consulta y mapear los resultados
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    TResult item = new();
                    Type tipo = typeof(TResult);
                    PropertyInfo[] propiedades = tipo.GetProperties();

                    foreach (PropertyInfo propiedad in propiedades)
                    {
                        try
                        {
                            if (reader[propiedad.Name] != DBNull.Value)
                            {
                                object valor = Convert.ChangeType(reader[propiedad.Name], propiedad.PropertyType);
                                propiedad.SetValue(item, valor);
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // La columna no existe en el resultado, ignoramos
                            continue;
                        }
                        catch (Exception ex)
                        {
                            // Log del error pero continuar con las otras propiedades
                            Debug.WriteLine($"Error al mapear la propiedad {propiedad.Name}: {ex.Message}");
                        }
                    }
                    resultado.Add(item);
                }

                return resultado;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Ejecuta una función escalar de base de datos que retorna un valor único de tipo genérico
        /// </summary>
        /// <typeparam name="TResult">Tipo de resultado esperado</typeparam>
        /// <param name="sqlFunction">Consulta SQL con formato "SELECT dbo.FuncionX(@param1, @param2)"</param>
        /// <param name="parameters">Lista de parámetros SqlParameter (opcional)</param>
        /// <param name="esTransaccion">Indica si se ejecuta dentro de una transacción</param>
        /// <returns>Valor único del tipo especificado o valor por defecto si es null</returns>
        public TResult EjecutarFunctionScalar<TResult>(string sqlFunction, List<SqlParameter>? parameters = null, bool esTransaccion = false)
        {
            try
            {
                using var connection = _dbContext.ObtenerConexionSql(esTransaccion);

                // Solo abrir conexión si no es transaccional
                if (!esTransaccion && connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                using var command = new SqlCommand(sqlFunction, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 600
                };

                // Agregar parámetros si existen
                if (parameters?.Count > 0)
                {
                    command.Parameters.AddRange(parameters.ToArray());
                }

                // Ejecutar y obtener resultado
                var resultado = command.ExecuteScalar();

                // Convertir resultado al tipo solicitado
                return ConvertirResultadoEscalar<TResult>(resultado);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al ejecutar función escalar: {sqlFunction}", ex);
            }
        }

        /// <summary>
        /// Convierte el resultado de ExecuteScalar al tipo genérico solicitado
        /// </summary>
        /// <typeparam name="TResult">Tipo de destino</typeparam>
        /// <param name="valor">Valor a convertir</param>
        /// <returns>Valor convertido al tipo especificado</returns>
        private static TResult ConvertirResultadoEscalar<TResult>(object? valor)
        {
            // Si el valor es null o DBNull
            if (valor == null || valor == DBNull.Value)
            {
                return default(TResult)!;
            }

            var tipoDestino = typeof(TResult);
            var tipoDestinoReal = Nullable.GetUnderlyingType(tipoDestino) ?? tipoDestino;

            try
            {
                // Si el tipo ya es el correcto
                if (tipoDestinoReal.IsAssignableFrom(valor.GetType()))
                {
                    return (TResult)valor;
                }

                // Conversiones específicas comunes
                if (tipoDestinoReal == typeof(bool) && valor is string strBool)
                {
                    return (TResult)(object)(strBool.Equals("S", StringComparison.OrdinalIgnoreCase) ||
                                           strBool.Equals("1") ||
                                           strBool.Equals("true", StringComparison.OrdinalIgnoreCase));
                }

                if (tipoDestinoReal == typeof(string))
                {
                    return (TResult)(object)valor.ToString()!;
                }

                // Conversión genérica usando Convert.ChangeType
                var valorConvertido = Convert.ChangeType(valor, tipoDestinoReal);
                return (TResult)valorConvertido;
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"No se pudo convertir el valor '{valor}' de tipo '{valor.GetType().Name}' a '{tipoDestino.Name}'", ex);
            }
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
                _dbContext.Commit();
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
            object resultado;
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

            if (parametros != null)
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
                _dbContext.Commit();
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
