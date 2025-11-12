using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;

namespace gc.api.core.Servicios.Ofertas
{
    public class ApiPresupuestoServicio : Servicio<EntidadBase>, IApiPresupuetoServicio
    {
        public ApiPresupuestoServicio(IUnitOfWork uow) : base(uow)
        {

        }

        public List<PresupuestoProductoDto> ObtenerDetallePresupuesto(string pre_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_PRESUP_P;

            var ps = new List<SqlParameter>();

            ps.Add(new SqlParameter("@pre_id", pre_id));

            var detalle = _repository.EjecutarLstSpExt<PresupuestoProductoDto>(sp, ps, true);

            return detalle;
        }

      

        public List<PresupE> ObtenerEstadosPresupuesto()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_PRESUP_ESTADOS;

            var ps = new List<SqlParameter>();


            var estados = _repository.EjecutarLstSpExt<PresupE>(sp, ps, true);

            return estados;
        }

        public List<PresupT> ObtenerTiposPresupuesto()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_PRESUP_TIPOS;

            var ps = new List<SqlParameter>();


            var estados = _repository.EjecutarLstSpExt<PresupT>(sp, ps, true);

            return estados;
        }

        public RespuestaDto ConfirmarPresupuesto(AbmPlusGenDto req)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_PRESUP_CONFIRMAR;

            var pres = JsonConvert.DeserializeObject<PresupuestoDto>(req.Json2);

            var ps = new List<SqlParameter>() {
                new SqlParameter("@abm", req.Abm),
                new SqlParameter("@pre_id", pres.pre_id??""),
                new SqlParameter("@pre_desc", pres.pre_descripcion),
                new SqlParameter("@pret_id", pres.pret_id),
                new SqlParameter("@pree_id", pres.pree_id),
                
                new SqlParameter("@pre_desde", pres.pre_vigencia_desde),
                new SqlParameter("@pre_hasta", pres.pre_vigencia_hasta),
                new SqlParameter("@pre_adm_id", pres.adm_id),

                new SqlParameter("cta_id", pres.cta_id??""),
                new SqlParameter("@pre_nombre", pres.pre_nombre??""),
                new SqlParameter("@pre_domicilio", pres.pre_domicilio ?? ""),

                new SqlParameter("@pre_obs_entrega", pres.pre_obs_entrega??""),
                new SqlParameter("@pre_obs_pago", pres.pre_obs_pago?? ""),

                new SqlParameter("@json_prod", req.Json),
                new SqlParameter("@usu_id", req.Usuario),
                new SqlParameter("@adm_id", req.Administracion),
                };

          
            var respuesta = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (respuesta.Count == 0)
            {
                return new RespuestaDto() { resultado = -1, resultado_msj = "No se Recepcionó respuesta del proceso." };
            }
            return respuesta[0];
        }

        public List<PresupuestoDto> ObtenerPresupuesto(string pre_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_PRESUP_DATOS;

            var ps = new List<SqlParameter>();

            ps.Add(new SqlParameter("@pre_id", pre_id));

            var presup = _repository.EjecutarLstSpExt<PresupuestoDto>(sp, ps, true);

            return presup;
        }

        public List<PresupuestoListDto> ObtenerListaPresupuestos(PresupuestoRequest req)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_PRESUP_LISTA;

            var ps = new List<SqlParameter>();
            if (string.IsNullOrEmpty(req.cli_list))
            {
                ps.Add(new SqlParameter("@c", false));
            }
            else
            {
                ps.Add(new SqlParameter("@c", true));
                ps.Add(new SqlParameter("@cta_list", req.cli_list));
            }

            if (req.Desde != default && req.Hasta != default)
            {
                ps.Add(new SqlParameter("@f", true));
                ps.Add(new SqlParameter("@desde", req.Desde));
                ps.Add(new SqlParameter("@hasta", req.Hasta));
            }
            else
            {
                ps.Add(new SqlParameter("@f", false));
            }

            if (string.IsNullOrEmpty(req.pree_list))
            {
                ps.Add(new SqlParameter("@e", false));
            }
            else
            {
                ps.Add(new SqlParameter("@e", true));
                ps.Add(new SqlParameter("@pree_list", req.pree_list));
            }

            if (string.IsNullOrEmpty(req.usu_list))
            {
                ps.Add(new SqlParameter("@u", false));
            }
            else
            {
                ps.Add(new SqlParameter("@u", true));
                ps.Add(new SqlParameter("@usu_list", req.usu_list));
            }

            if (string.IsNullOrEmpty(req.adm_list))
            {
                ps.Add(new SqlParameter("@a", false));
            }
            else
            {
                ps.Add(new SqlParameter("@a", true));
                ps.Add(new SqlParameter("@adm_list", req.adm_list));
            }

            ps.Add(new SqlParameter("@registros", req.Registros));
            ps.Add(new SqlParameter("@pagina", req.Pagina));

            var presupuestos = _repository.EjecutarLstSpExt<PresupuestoListDto>(sp, ps, true);

            return presupuestos;
        }
    }
}
