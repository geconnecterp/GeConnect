using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.EntidadesComunes;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;
using System.Net;

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
                return (false, ConstantesGC.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Rb_Compte"));
            }
            if (string.IsNullOrEmpty(orden.Adm_Id))
            {
                return (false, ConstantesGC.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Adm_Id"));
            }
            if (string.IsNullOrEmpty(orden.Caja_Id))
            {
                return (false, ConstantesGC.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Caja_Id"));
            }
            if (string.IsNullOrEmpty(orden.Bill_Id))
            {
                return (false, ConstantesGC.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Bill_Id"));
            }
            if (string.IsNullOrEmpty(orden.Cuit))
            {
                return (false, ConstantesGC.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Cuit"));
            }
            if (string.IsNullOrEmpty(orden.Tco_Id))
            {
                return (false, ConstantesGC.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Tco_Id"));
            }
            if (string.IsNullOrEmpty(orden.Cm_Compte))
            {
                return (false, ConstantesGC.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Cm_Compte"));
            }
            if (orden.Bo_Importe == default)
            {
                return (false, ConstantesGC.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Bo_Importe"));
            }
            if (string.IsNullOrEmpty(orden.Bo_Clave))
            {
                return (false, ConstantesGC.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Bo_Clave"));
            }
            if (string.IsNullOrEmpty(orden.Ip))
            {
                return (false, ConstantesGC.MensajeError.ERR_VALOR_CAMPO_CRITICO.Replace("@campo", "Ip"));
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
                return (false, ConstantesGC.MensajeError.ERR_AL_INSERTAR);

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
                return (false, ConstantesGC.MensajeError.ERR_AL_ACTUALIZAR);
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
                return (false, ConstantesGC.MensajeError.ERR_AL_ACTUALIZAR);
            }
        }

        public override PagedList<BilleteraOrden> GetAll(QueryFilters filters)
        {
            filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var billeteras_ordeness = GetAllIq();
            billeteras_ordeness = billeteras_ordeness.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    billeteras_ordeness = billeteras_ordeness.Where(r => r.Bo_Id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Bo_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Rb_Compte.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Adm_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Caja_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Bill_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Boe_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Cuit.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Tco_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Cm_Compte.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Bo_Clave.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Bo_Id_Ext.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Bo_Notificado_Desc.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordeness = billeteras_ordeness.Where(r => r.Ip.Contains(filters.Search));
            }

            var paginas = PagedList<BilleteraOrden>.Create(billeteras_ordeness, filters.PageNumber ?? 1, filters.PageSize ?? 20);

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
        public (bool, string) VerificaPago(string ordenId)
        {
            var sp = ConstantesGC.StoredProcedures.SP_BILLETERAORD_VERIFICA_PAGO;
            List<SqlParameter>? ps = new List<SqlParameter>();

            ps.Add(new SqlParameter("@orden_id", ordenId));


            var res = _repository.InvokarSpScalar(sp, ps);

            if (res.GetType().Name.Equals("DBNull"))
            {
                return (false, "");
            }
            else
            {
                return (true, (string)res);
            }
        }
    }
}
