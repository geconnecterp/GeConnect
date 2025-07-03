using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Asientos;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Asientos;

using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.Asientos
{
    public class AsientoServicio : Servicio<EntidadBase>, IAsientoServicio
    {
        
        public AsientoServicio(IUnitOfWork uow):base(uow)
        {
        }

        public List<AsientoAjusteDto> ObtenerAsientosAjuste(int eje_nro)
        {
            var sp = ConstantesGC.StoredProcedures.SP_ASIENTO_AJUSTE_INFLACION;
            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@eje_nro", eje_nro)
            };

            var lista = _repository.EjecutarLstSpExt<AsientoAjusteDto>(sp, ps, true);
            return lista;
        }

        public List<AsientoAjusteCcbDto> ObtenerAsientosAjusteCcb(int eje_nro, string ccb_id, bool todas)
        {
            var sp = ConstantesGC.StoredProcedures.SP_ASIENTO_AJUSTE_CCB;
            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@eje_nro", eje_nro),
                new SqlParameter("@ccb_id",ccb_id),
                new SqlParameter("@todas",todas)
            };

            var lista = _repository.EjecutarLstSpExt<AsientoAjusteCcbDto>(sp, ps, true);
            return lista;
        }

        public List<EjercicioDto> ObtenerEjercicios()
        {
            var sp = ConstantesGC.StoredProcedures.SP_EJERCICIOS_LISTA;
            var ps = new List<SqlParameter>();
            var lista = _repository.EjecutarLstSpExt<EjercicioDto>(sp, ps, true);
            return lista;
        }

        public List<TipoAsientoDto> ObtenerTiposAsiento()
        {
            var sp = ConstantesGC.StoredProcedures.SP_TIPO_ASIENTO;
            var ps = new List<SqlParameter>();

            var lista = _repository.EjecutarLstSpExt<TipoAsientoDto>(sp, ps, true);
            return lista;
        }

        public List<UsuAsientoDto> ObtenerUsuariosDeEjercicio(int eje_nro)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CONTA_USU_ASIENTOS;
            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@eje_nro", eje_nro)
            };

            var lista = _repository.EjecutarLstSpExt<UsuAsientoDto>(sp, ps, true);
            return lista;
        }


    }
}
