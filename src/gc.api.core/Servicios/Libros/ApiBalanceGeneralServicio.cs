using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Libros;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;

namespace gc.api.core.Servicios.Libros
{
    public class ApiBalanceGeneralServicio : Servicio<EntidadBase>, IApiBalanceGeneralServicio
    {
        public ApiBalanceGeneralServicio(IUnitOfWork uow) : base(uow)
        {

        }
        public List<BalanseGrDto> ObtenerBalanceGeneral(int eje_nro)
        {
            var sp = ConstantesGC.StoredProcedures.SP_BALANCE_GR_LISTA;
            var ps = new List<SqlParameter>();

            // Evaluar y agregar parámetros al procedimiento almacenado
            ps.Add(new SqlParameter("@eje_nro", eje_nro));
           

            // Ejecutar el procedimiento almacenado y devolver los resultados
            return _repository.EjecutarLstSpExt<BalanseGrDto>(sp, ps, true);
        }
    }
}
