using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Importacion;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Importacion;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.Importacion
{
    public class ApiImportarServicio : Servicio<EntidadBase>, IApiImportarServicio
    {
        public ApiImportarServicio(IUnitOfWork uow) : base(uow)
        {
            
        }

        public List<ProveedorPerfilDB> ObtenerPerfildePreciosCliente(string ctaId)
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_PERFIL_PRECIOS_CLIENTE;

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@cta_id", ctaId)
            };

            List<ProveedorPerfilDB> resultadoDB = _repository.EjecutarLstSpExt<ProveedorPerfilDB>(sp, ps, true);
            
            return resultadoDB;
        }

        public List<PrecioFileDatos> ObtenerPrecioFileDatos()
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_PRECIO_FILE_DATOS;
            var ps = new List<SqlParameter>();
            List<PrecioFileDatos> resp = _repository.EjecutarLstSpExt<PrecioFileDatos>(sp, ps, true);
            return resp;
        }

        public RespuestaDto ConfirmarPerfilPrecioPerfil(string ctaId, string usuario, string admin,string json)
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_PERFIL_PRECIOS_CONFIRMA;
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@cta_id", ctaId),
                new SqlParameter("@usu_id", usuario),
                new SqlParameter("@adm_id", admin),
                new SqlParameter("@json", json)
            };

            List<RespuestaDto> resultado = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (resultado == null || !resultado.Any())
            {
                return new RespuestaDto
                {
                    resultado = -1,
                    resultado_msj = "No se pudo confirmar el perfil de precios. Verifique los datos ingresados."
                };
            }
            return resultado.First();
        }

    }
}
