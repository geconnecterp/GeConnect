using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace gc.api.core.Servicios.Ofertas
{
    public class ApiEtiquetaServicio : Servicio<EntidadBase>, IApiEtiquetaServicio
    {
        private readonly ILogger<ApiEtiquetaServicio> _logger;
        public ApiEtiquetaServicio(IUnitOfWork uow,
            ILogger<ApiEtiquetaServicio> logger) : base(uow)
        {
            _logger = logger;
        }

        public RespuestaDto ConfirmarCargaPrevia(AbmGenDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_IE_CARGA_CONTEOS;
            var ps = new List<SqlParameter> {
                new SqlParameter("@json", req.Json),
                new SqlParameter("adm_id", req.Administracion),
                new SqlParameter("usu_id", req.Usuario)
            };

            var result = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (result == null || result.Count == 0)
            {
                return new() { resultado = -1, resultado_msj = "No se logró obtener confirmación de la operación." };
            }
            return result[0];
        }

        public RespuestaDto ConfirmarImpresionEtiqueta(string json, string adm, string usu)
        {
            var sp = ConstantesGC.StoredProcedures.SP_IE_CONFIRMA;
            var ps = new List<SqlParameter> {
                new SqlParameter("@json_p", json),
                new SqlParameter("adm_id",adm),
                new SqlParameter("usu_id",usu)
            };

            var result = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (result == null || result.Count == 0)
            {
                return new() { resultado = -1, resultado_msj = "No se logró obtener confirmación de la operación." };
            }
            return result[0];
        }

        public List<CargaPreviaDto> ObtenerCargaPreviaUsuario(string adm_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CARGA_PREVIA;

            //var ps = new List<SqlParameter>() {
            //    new SqlParameter("@adm_id", adm_id)
            //};

            List<SqlParameter> ps = [];
            var p = new SqlParameter("@adm_id", SqlDbType.VarChar, 4);
            p.Value = adm_id;
            //ps.Add(new SqlParameter("@adm_id", adm_id));
            ps.Add(p);

            var datos = _repository.EjecutarLstSpExt<CargaPreviaDto>(sp, ps, true);

            return datos;
        }

        public List<EtiquetaDto> ObtenerDatosParaEtiqueta(string json, int etiq, string adm, string usu)
        {
            var sp = ConstantesGC.StoredProcedures.SP_IE_DATOS;

            List<SqlParameter> ps = new List<SqlParameter>
            {
                new SqlParameter("@json_p",json),
                new SqlParameter("@etiqueta",etiq),
                new SqlParameter("@adm_id",adm),
                new SqlParameter("@usu_id",usu)
            };

            var etiquetas = _repository.EjecutarLstSpExt<EtiquetaDto>(sp, ps, true);
            return etiquetas;
        }

        public List<IEDetalleDto> ObtenerDetalleEtiquetas(QueryFilters filters)
        {
            var sp = ConstantesGC.StoredProcedures.SP_IE_LISTA;
            List<SqlParameter> ps = [];

            var filtraModificados = filters.FechaD.HasValue && filters.FechaD.Value != DateTime.MinValue &&
                                    filters.FechaH.HasValue && filters.FechaH.Value != DateTime.MinValue;

            if (filtraModificados && filters.FechaD!.Value.Date > filters.FechaH!.Value.Date)
            {
                throw new ArgumentException("La fecha desde no puede ser posterior a la fecha hasta.");
            }

            ps.Add(new SqlParameter("@mod", filtraModificados));
            ps.Add(new SqlParameter("@mod_d", SqlDbType.Date)
            {
                Value = filtraModificados ? filters.FechaD!.Value.Date : DBNull.Value
            });
            ps.Add(new SqlParameter("@mod_h", SqlDbType.Date)
            {
                Value = filtraModificados ? filters.FechaH!.Value.Date : DBNull.Value
            });
            ps.Add(new SqlParameter("@sin_imprimir", filters.Opt1));
            ps.Add(new SqlParameter("@oferta", filters.Opt2));
            var tiposOferta = filters.Opt2 == true
                ? filters.OfertaList
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                : Enumerable.Empty<string>();
            ps.Add(new SqlParameter("@oferta_list", string.Join(",", tiposOferta)));

            //previa de usuario
            if (!string.IsNullOrEmpty(filters.StrOpt03))
            {
                ps.Add(new SqlParameter("@previa", true));
                ps.Add(new SqlParameter("@previa_usu_id", filters.StrOpt03));
            }
            else
            {
                ps.Add(new SqlParameter("@previa", false));
                ps.Add(new SqlParameter("@previa_usu_id", ""));
            }

            //proveedores 
            if (filters.Rel01 != null && filters.Rel01.Count > 0)
            {
                var provs = string.Join(",", filters.Rel01);
                ps.Add(new SqlParameter("@prov", true));
                ps.Add(new SqlParameter("@prov_list", provs));
            }
            else
            {
                ps.Add(new SqlParameter("@prov", false));
                ps.Add(new SqlParameter("@prov_list", string.Empty));
            }

            // Rel03 representa Familias en el filtro compartido.
            if (filters.Rel03 != null && filters.Rel03.Count > 0)
            {
                var pgs = string.Join(",", filters.Rel03.Select(x => x.Id));
                ps.Add(new SqlParameter("@pg", true));
                ps.Add(new SqlParameter("@pg_list", pgs));
            }
            else
            {
                ps.Add(new SqlParameter("@pg", false));
                ps.Add(new SqlParameter("@pg_list", string.Empty));
            }

            // Rel02 representa Rubros en el filtro compartido.
            if (filters.Rel02 != null && filters.Rel02.Count > 0)
            {
                var rubs = string.Join(",", filters.Rel02);
                ps.Add(new SqlParameter("@rub", true));
                ps.Add(new SqlParameter("@rub_list", rubs));
            }
            else
            {
                ps.Add(new SqlParameter("@rub", false));
                ps.Add(new SqlParameter("@rub_list", string.Empty));
            }

            ps.Add(new SqlParameter("@adm_id", filters.Adm_id));
            ps.Add(new SqlParameter("@usu_id", filters.Usu_id));
            var datos = _repository.EjecutarLstSpExt<IEDetalleDto>(sp, ps, true);
            return datos;
        }


    }
}
