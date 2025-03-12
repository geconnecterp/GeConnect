using gc.api.infra.Datos.Contratos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace gc.api.infra.Datos.Implementacion
{
    public class DataConnectionContext : IDataConnectionContext
    {
        private readonly GeConnectContext _context;
        private readonly ManejadorDeTransacciones manejador;

        public DataConnectionContext(GeConnectContext context)
        {
            _context = context;
            manejador = new ManejadorDeTransacciones();
        }

        public SqlConnection ObtenerConexionSql(bool esTransaccional = false)
        {
            if (esTransaccional)
            {
                //verifico si se creo la transacción
                if (manejador.sqlTransaction == null)
                {
                    ///no existe la transacción por lo que se debe crear la conexion. Si existe una conexion previa 
                    ///se debe verificar si se encuentra cerrada para generar la transacción.
                    ///
                    GenerarConexion();

                    //debo generar la transaccion por lo que debo abrir la conexion para instanciar la transacción
                    manejador.sqlConnection.Open();
                    manejador.sqlTransaction = manejador.sqlConnection.BeginTransaction();
                }
            }
            else
            {
                GenerarConexion();
            }

            return manejador.sqlConnection;
        }

        private void GenerarConexion()
        {
            if (manejador.sqlConnection == null || string.IsNullOrWhiteSpace(manejador.sqlConnection.ConnectionString))
            {
                manejador.sqlConnection = new SqlConnection(_context.Database.GetConnectionString());
            }
            if (manejador.sqlConnection.State == ConnectionState.Open)
            {
                manejador.sqlConnection.Close();
            }
        }

        public void CerrarConexion(bool grabarCambios = false)
        {
            if (grabarCambios)
            {
                GrabarCambios();
            }
            if (manejador.sqlTransaction != null)
            {
                manejador.sqlTransaction = null;
            }
            if (manejador.sqlConnection.State == ConnectionState.Open)
            {
                manejador.sqlConnection.Close();
            }
        }

        public SqlCommand ObtenerCommandSql(SqlConnection cnn, CommandType commandType)
        {
            SqlCommand cmd = new SqlCommand()
            {
                Connection = cnn,
                CommandType = commandType,
                CommandTimeout = 180
            };
            if (manejador.sqlTransaction != null)
                cmd.Transaction = manejador.sqlTransaction;

            return cmd;
        }

        public SqlDataReader ObtenerDatosDelCommand(SqlCommand cmd)
        {
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public void GrabarCambios()
        {
            try
            {
                if (manejador.sqlTransaction != null)
                {
                    manejador.sqlTransaction.Commit();
                }
            }
            catch (Exception )
            {

                //  _logger.Log(LogLevel.Error, $"DatabaseContext - Error al grabar los datos: {ex.Message}");
                if (manejador.sqlTransaction != null)
                {
                    manejador.sqlTransaction.Rollback();
                }
                throw;
            }
        }

        public void DeshacerCambios()
        {
            try
            {
                if (manejador.sqlTransaction != null)
                {
                    manejador.sqlTransaction.Rollback();
                }
            }
            catch (Exception )
            {
                // _logger.Log(LogLevel.Error, $"DatabaseContext - Error al Deshacer los datos: {ex.Message}");
                throw;
            }
        }

        public List<SqlParameter> InferirParametros<T>(T entidad, IEnumerable<string> excluir = null)
        {
            var parametros = new List<SqlParameter>();

            if (excluir == null) excluir = new List<string>();

            var t = typeof(T);

            var properties = t.GetProperties().Where(p => !excluir.Contains(p.Name));

            foreach (var prop in properties)
            {
                var value = prop.GetValue(entidad, null);
                if (value == null) { continue; }
                var name = "@" + prop.Name;

                parametros.Add(new SqlParameter(name, value));
            }

            return parametros;
        }

        public void Dispose()
        {

            GC.SuppressFinalize(this);
        }

        
    }

    public class ManejadorDeTransacciones
    {
        public SqlConnection sqlConnection = null;
        public SqlTransaction sqlTransaction = null;

        public bool HasConectionOpen()
        {
            if (sqlConnection == null)
                return false;
            else
                if (sqlConnection.State == System.Data.ConnectionState.Open)
                return true;
            else
                return false;
        }
    }
}