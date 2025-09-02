using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Importacion;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Importacion;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Productos.Actualiza;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.Importacion
{
    public class ApiImportarServicio : Servicio<EntidadBase>, IApiImportarServicio
    {
        public ApiImportarServicio(IUnitOfWork uow) : base(uow)
        {

        }
        #region Metodos de Importación

        public List<MapeoColumnaDto> ObtenerPerfilDeProveedor(string ctaId)
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_PERFIL_PROV;

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@cta_id", ctaId)
            };

            List<MapeoColumnaDto> resultadoDB = _repository.EjecutarLstSpExt<MapeoColumnaDto>(sp, ps, true);

            return resultadoDB;
        }

        public List<PrecioFileDatos> ObtenerPrecioFileDatos()
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_PRECIO_FILE_DATOS;
            var ps = new List<SqlParameter>();
            List<PrecioFileDatos> resp = _repository.EjecutarLstSpExt<PrecioFileDatos>(sp, ps, true);
            return resp;
        }

        public List<RespuestaCPDto> CargarImportacionPrecioPerfil(AbmPlusGenDto req)
        {
            string sp;
            if (req.Abm.Equals('A'))
            {
                sp = ConstantesGC.StoredProcedures.SP_PROD_FILE_CARGA;
            }
            else
            {
                sp = ConstantesGC.StoredProcedures.SP_PROD_FILE_CONFIRMA;
            }

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@cta_id", req.Objeto),
                new SqlParameter("@usu_id", req.Usuario),
                new SqlParameter("@adm_id", req.Administracion),
            };

            if (req.Abm.Equals('A'))
            {
                ps.Add(new SqlParameter("@json", req.Json));
            }
            else
            {
                ps.Add(new SqlParameter("@idfile", req.IdFile));
                ps.Add(new SqlParameter("@solo_plista", req.SoloPLista));
                ps.Add(new SqlParameter("@nuevos", req.Nuevos));
                ps.Add(new SqlParameter("@datos_logisticos", req.DatosLogisticos));
                ps.Add(new SqlParameter("@inactivos", req.Inactivos));
                ps.Add(new SqlParameter("@vaciatmp", req.vaciarTemporal));

            }
            List<RespuestaCPDto> resultado = _repository.EjecutarLstSpExt<RespuestaCPDto>(sp, ps, true);

            return resultado;
        }

        public RespuestaDto CargaPerfilCuenta(string ctaId, string usu, string adm, string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new();
            }
            var sp = ConstantesGC.StoredProcedures.SP_PROD_PERFIL_CARGA;
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@cta_id", ctaId),
                new SqlParameter("@usu_id", usu),
                new SqlParameter("@adm_id", adm),
                new SqlParameter("@json", json)
            };

            List<RespuestaDto> resultado = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (resultado.Count == 0)
            {
                return new RespuestaDto { resultado = -1, resultado_msj = "Hubo algun problema al intentar cargar el perfil del Proveedor." };
            }
            return resultado.First();
        }
        #endregion

        #region Metodos para la Actualización de Precios

        public RespuestaDto ConfirmarActualizacionPrecioProductosDeProveedor(AbmGenDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_ACTUALIZA_CONFIRMAR;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@usu_id",req.Usuario),
                new SqlParameter("@adm_id",req.Administracion),
                new SqlParameter("@json_cta",req.Json)
            };
            List<RespuestaDto> resultado = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (resultado.Count == 0)
            {
                return new RespuestaDto { resultado = -1, resultado_msj = "No se pudo procesar la solicitud" };
            }
            return resultado.First();
        }

        public List<ProductoDetalleDto> ObtenerProductosDelProveedorParaActualizar(QueryFilters filters)
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_ACTUALIZA_PRECIO_PROV_D;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@cta_id",filters.Id),
                new SqlParameter("@registros",filters.Registros),
                new SqlParameter("@pagina",filters.Pagina),
                new SqlParameter("@ordenar",filters.Sort)
            };
            List<ProductoDetalleDto> resultado = _repository.EjecutarLstSpExt<ProductoDetalleDto>(sp, ps, true);
            return resultado;
        }

        public List<ActualizaProveedorDto> ObtenerProveedoresConProductosParaActualizar()
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_ACTUALIZA_PRECIO_PROV;

            var ps = new List<SqlParameter>();
            List<ActualizaProveedorDto> resultado = _repository.EjecutarLstSpExt<ActualizaProveedorDto>(sp, ps, true);
            return resultado;
        }

        #endregion

    }
}
