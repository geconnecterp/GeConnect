using DocumentFormat.OpenXml.Office2010.Excel;
using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Servicios.Reportes;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.Dtos.Productos.PromoCombo;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace gc.api.core.Servicios.Ofertas
{
    public class ApiPromoComboServicio : Servicio<EntidadBase>, IApiPromoComboServicio
    {
        public ApiPromoComboServicio(IUnitOfWork uow) : base(uow)
        {

        }

        public List<ComboCanalDto> ObtenerCanalesDeCombo(string id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_COMBO_CANALES;
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@cmb_id", id)
            };
            var lista = _repository.EjecutarLstSpExt<ComboCanalDto>(sp, ps, true);
            if (lista.Count == 0)
            {
                throw new Exception("No se encontraron canales para el combo solicitado");
            }
            return lista;
        }

        /// <summary>
        /// Permite obtener los estados para el combo
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<ComboEstadoDto> ObtenerComboEstado()
        {
            var sp = ConstantesGC.StoredProcedures.SP_COMBO_ESTADO;
            var ps = new List<SqlParameter>();

            var lista = _repository.EjecutarLstSpExt<ComboEstadoDto>(sp, ps, true);
            if (lista.Count == 0)
            {
                throw new Exception("No se encontraron estados para el combo");
            }

            return lista;

        }

        public ComboDatosDto ObtenerComboPorId(string id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_COMBO_DATOS;
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@cmb_id", id)
            };
            var instancia = _repository.EjecutarLstSpExt<ComboDatosDto>(sp, ps, true);
            if (instancia.Count == 0)
            {
                throw new Exception("No se encontro el combo solicitado");
            }
            return instancia[0];
        }

        /// <summary>
        /// Permite obtener los tipos para el combo
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<ComboTipoDto> ObtenerComboTipo()
        {
            var sp = ConstantesGC.StoredProcedures.SP_COMBO_TIPO;
            var ps = new List<SqlParameter>();
            var lista = _repository.EjecutarLstSpExt<ComboTipoDto>(sp, ps, true);
            if (lista.Count == 0)
            {
                throw new Exception("No se encontraron tipos para el combo");
            }
            return lista;
        }
        /// <summary>
        /// Devuelve el detalle de las PROMOS Y COMBOS segun los filtros y paginacion
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<ComboListaDto> ObtenerDetalleDeCombos(QueryFilters req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_COMBO_LISTA;
            var ps = new List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(req.Tipo))
            {
                ps.Add(new SqlParameter("@tipo", true));
                ps.Add(new SqlParameter("@cmb_tipo", req.Tipo));
            }
            else
            {
                ps.Add(new SqlParameter("@tipo", false));
            }
            if (!string.IsNullOrWhiteSpace(req.Estado))
            {
                ps.Add(new SqlParameter("@estado", true));
                ps.Add(new SqlParameter("@cmb_estado", req.Estado));
            }
            else
            {
                ps.Add(new SqlParameter("@estado", false));
            }

            ps.Add(new SqlParameter("@registros", req.Registros));
            ps.Add(new SqlParameter("@pagina", req.Pagina));

            var lista = _repository.EjecutarLstSpExt<ComboListaDto>(sp, ps, true);
            return lista;
        }

        public List<ComboProductoDto> ObtenerProductosDeCombo(string id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_COMBO_PRODUCTOS;
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@cmb_id", id)
            };
            var lista = _repository.EjecutarLstSpExt<ComboProductoDto>(sp, ps, true);
            if (lista.Count == 0)
            {
                throw new Exception("No se encontraron productos para el combo solicitado");
            }
            return lista;
        }

        public List<ComboSustitutoDto> ObtenerProductosSustitutosDeCombo(string id, string p_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_COMBO_SUSTITUTOS;
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@cmb_id", id),
                new SqlParameter("@p_id",p_id)
            };
            var lista = _repository.EjecutarLstSpExt<ComboSustitutoDto>(sp, ps, true);

            return lista;
        }

        public RespuestaDto ConfirmarCombo(AbmPlusGenDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_COMBO_CONFIRMAR;

            ComboDatosDto datos = JsonConvert.DeserializeObject<ComboDatosDto>(req.Json4);
            if (datos == null)
            {
                return new RespuestaDto { resultado = -1, resultado_msj = "No se han recepcionado los datos del COMBO/PROMO. " };
            }



            var ps = new List<SqlParameter>
            {
                new SqlParameter("@cmb_id", datos.cmb_id),
                new SqlParameter("@cmb_desc",datos.cmb_desc),
                new SqlParameter("@cmb_desde",datos.cmb_desde),
                new SqlParameter("@cmb_hasta",datos.cmb_hasta),
                new SqlParameter("@cmb_tipo",datos.cmb_tipo),
                new SqlParameter("@cmb_estado",datos.cmb_estado),
                new SqlParameter("@json_canales",req.Json2),
                new SqlParameter("@json_prod",req.Json),
                new SqlParameter("@json_prod_sus",req.Json3),
                new SqlParameter("@usu_id",req.Usuario),
                new SqlParameter("@adm_id",req.Administracion),
            };

            var resp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);

            if (resp == null || resp.Count == 0)
            {
                return new() { resultado = -1, resultado_msj = "No es ha podido determinar el resultado del proceso. verifique e intentelo nuevamente." };
            }
            return resp[0];
        }
    }
}
