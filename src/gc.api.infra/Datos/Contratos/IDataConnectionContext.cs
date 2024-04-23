using Microsoft.Data.SqlClient;
using System.Data;


namespace gc.api.infra.Datos.Contratos
{
    public interface IDataConnectionContext
    {
        void CerrarConexion(bool grabarCambios = false);
        void DeshacerCambios();
        void Dispose();
        void GrabarCambios();
        List<SqlParameter> InferirParametros<T>(T entidad, IEnumerable<string> excluir = null);
        SqlCommand ObtenerCommandSql(SqlConnection cnn, CommandType commandType);
        SqlConnection ObtenerConexionSql(bool esTransaccional = false);
        SqlDataReader ObtenerDatosDelCommand(SqlCommand cmd);
    }
}