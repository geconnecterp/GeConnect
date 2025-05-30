using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class BilleteraOrdenServicio : Servicio<BilleteraOrden>, IBilleteraOrdenServicio
    {
        public BilleteraOrdenServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public (bool, string) CargarOrden(BilleteraOrden orden)
        {
            if (string.IsNullOrEmpty(orden.Rb_Compte))
            {
                return (false, gc.infraestructura.Constantes.Constantes.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Rb_Compte"));
            }
            if (string.IsNullOrEmpty(orden.Adm_Id))
            {
                return (false, infraestructura.Constantes.Constantes.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Adm_Id"));
            }
            if (string.IsNullOrEmpty(orden.Caja_Id))
            {
                return (false, infraestructura.Constantes.Constantes.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Caja_Id"));
            }
            if (string.IsNullOrEmpty(orden.Bill_Id))
            {
                return (false, infraestructura.Constantes.Constantes.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Bill_Id"));
            }
            if (string.IsNullOrEmpty(orden.Cuit))
            {
                return (false, infraestructura.Constantes.Constantes.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Cuit"));
            }
            if (string.IsNullOrEmpty(orden.Tco_Id))
            {
                return (false, infraestructura.Constantes.Constantes.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Tco_Id"));
            }
            if (string.IsNullOrEmpty(orden.Cm_Compte))
            {
                return (false, infraestructura.Constantes.Constantes.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Cm_Compte"));
            }
            if (orden.Bo_Importe == default)
            {
                return (false, infraestructura.Constantes.Constantes.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Bo_Importe"));
            }
            if (string.IsNullOrEmpty(orden.Bo_Clave))
            {
                return (false, infraestructura.Constantes.Constantes.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Bo_Clave"));
            }
            if (string.IsNullOrEmpty(orden.Ip))
            {
                return (false, infraestructura.Constantes.Constantes.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Ip"));
            }

            var sp = ConstantesGC.StoredProcedures.SP_BILLETERAORD_CARGA;

            List<string> excluir = new List<string>() { "Bo_Id", "Boe_Id", "Bo_Carga", "Bo_Id_Ext", "Bo_Notificado", "Bo_Notificado_Desc" };
            var ps = _repository.InferirParametros(orden, excluir).ToList();

            object res = _repository.InvokarSpScalar(sp, ps);

            if (res != null)
            {
                return (true, (string)res);
            }
            else
            {
                return (false, infraestructura.Constantes.Constantes.MensajeError.ERR_AL_INSERTAR);

            }
        }

        public (bool, string) OrdenNotificado(OrdenNotificado ordenNotificado)
        {
            var sp = ConstantesGC.StoredProcedures.SP_BILLETERAORD_NOTIFICADO;
            List<SqlParameter>? ps = new List<SqlParameter>();

            ps.Add(new SqlParameter("@orden_id", ordenNotificado.Orden_Id));
            ps.Add(new SqlParameter("@orden_notificada_ok", ordenNotificado.Orden_Notificada_Ok));
            ps.Add(new SqlParameter("@orden_id_ext", ordenNotificado.Orden_Id_Ext));
            ps.Add(new SqlParameter("@response_mepa", ordenNotificado.ResponseMepa));
            ps.Add(new SqlParameter("@status", ordenNotificado.Status));

            var res = _repository.InvokarSpScalar(sp, ps);

            if ((int)res == 0)
            {
                return (true, "OK");
            }
            else
            {
                return (false, infraestructura.Constantes.Constantes.MensajeError.ERR_AL_ACTUALIZAR);
            }
        }

        public (bool, string) OrdenRegistro(OrdenRegistro ordenRegistro)
        {
            var sp = ConstantesGC.StoredProcedures.SP_BILLETERAORD_REGISTRA;
            List<SqlParameter>? ps = new List<SqlParameter>();

            ps.Add(new SqlParameter("@orden_id", ordenRegistro.Orden_Id));
            ps.Add(new SqlParameter("@orden_solicitada_ok", ordenRegistro.Orden_Solicitada_Ok));
            ps.Add(new SqlParameter("@orden_id_ext", ordenRegistro.Orden_Id_Ext));

            var res = _repository.InvokarSpScalar(sp, ps);

            if ((int)res == 0)
            {
                return (true, "OK");
            }
            else
            {
                return (false, infraestructura.Constantes.Constantes.MensajeError.ERR_AL_ACTUALIZAR);
            }
        }

        public override PagedList<BilleteraOrden> GetAll(QueryFilters filters)
        {
            filters.Pagina = filters.Pagina == default ? _pagSet.DefaultPageNumber : filters.Pagina;
            filters.Registros = filters.Registros == default ? _pagSet.DefaultPageSize : filters.Registros;

            var billeteras_ordeness = GetAllIq();
            billeteras_ordeness = billeteras_ordeness.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    billeteras_ordeness = billeteras_ordeness.Where(r => r.Bo_Id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Bo_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Rb_Compte.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Adm_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Caja_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Bill_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Boe_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Cuit.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Tco_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Cm_Compte.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Bo_Clave.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Bo_Id_Ext.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Bo_Notificado_Desc.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Ip.Contains(filters.Buscar));
            }

            var paginas = PagedList<BilleteraOrden>.Create(billeteras_ordeness, filters.Pagina ?? 1, filters.Registros ?? 20);

            return paginas;
        }

        public override BilleteraOrden Find(object id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_BILLETERAORD_OBTENER_BY_ID;
            List<SqlParameter>? ps = new List<SqlParameter>();

            ps.Add(new SqlParameter("@bo_id", id.ToString()));


            var res = _repository.InvokarSp2Lst(sp, ps, true);

            if (res.Count == 0)
            {
                return null;
            }
            return res.First();
        }
        public (bool, (string, string)) VerificaPago(string ordenId)
        {
            var sp = ConstantesGC.StoredProcedures.SP_BILLETERAORD_VERIFICA_PAGO;
            List<SqlParameter>? ps = new List<SqlParameter>();

            ps.Add(new SqlParameter("@orden_id", ordenId));


            var res = _repository.InvokarSpScalar(sp, ps);

            if (res.GetType().Name.Equals("DBNull"))
            {
                return (false, ("", ""));
            }
            else
            {
                var datos = res.ToString().Split('|');
                return (true, (datos[0], datos[1]));
            }
        }
    }
}
