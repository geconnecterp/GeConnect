using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;

namespace gc.api.core.Servicios.Libros
{
    public class ApiSumaSaldoServicio : Servicio<EntidadBase>, IApiSumaSaldoServicio
    {
        public ApiSumaSaldoServicio(IUnitOfWork uow) : base(uow)
        {

        }
        public List<BSumaSaldoRegDto> ObtenerBalanceSumaSaldos(LibroFiltroDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_BALANCE_SS_LISTA;
            var ps = new List<SqlParameter>();

            // Evaluar y agregar parámetros al procedimiento almacenado
            ps.Add(new SqlParameter("@eje_nro", req.eje_nro));

            ps.Add(new SqlParameter("@desde", req.desde));
            ps.Add(new SqlParameter("@hasta", req.hasta));


            ps.Add(new SqlParameter("@incluye_tmp",req.incluirTemporales));

            // Ejecutar el procedimiento almacenado y devolver los resultados
            return _repository.EjecutarLstSpExt<BSumaSaldoRegDto>(sp, ps, true);
        }
    }
}
