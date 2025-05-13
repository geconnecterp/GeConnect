using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Contable;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Contabilidad;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace gc.api.core.Servicios.Contable
{
    public class ABMPlanCuentaServicio : Servicio<PlanContable>, IABMPlanCuentaServicio
    {
        public ABMPlanCuentaServicio(IUnitOfWork uow) : base(uow)
        {

        }

        public PlanCuentaDto ObtenerCuenta(string ccb_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CCB_PLANCUENTAS_DATO;

            var ps = new List<SqlParameter>();
            ps.Add(new SqlParameter("@ccb_id", ccb_id));

            List<PlanCuentaDto> res = _repository.EjecutarLstSpExt<PlanCuentaDto>(sp, ps, true);
            if (res.Count == 0)
            {
                throw new NegocioException("No se logró encontrar la Cuenta solicitada. Si el problema continua avise al Administrador del Sistema.");
            }
            else
            {
                return res.First();
            }
        }

        public List<PlanCuentaDto> ObtenerPlanCuenta(QueryFilters filtros)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CCB_PLANCUENTAS_LISTA;

            var ps = new List<SqlParameter>();

            //debo cargar aca todos los filtros sobre los parametros a utilizar
            if (!string.IsNullOrEmpty(filtros.Id))
            {
                ps.Add(new SqlParameter("@id", true));
                //hay un id de producto. se habilita la seccion de productos
                ps.Add(new SqlParameter("@id_d", filtros.Id));

                if (!string.IsNullOrEmpty(filtros.Id2))
                {
                    ps.Add(new SqlParameter("@id_h", filtros.Id2));
                }
                else
                {
                    ps.Add(new SqlParameter("@id_h", filtros.Id));
                }
            }
            else
            {
                ps.Add(new SqlParameter("@id", false));
            }

            //se carga si es necesario los parametros del sp
            if (!string.IsNullOrEmpty(filtros.Buscar))
            {
                ps.Add(new SqlParameter("@deno", true));
                ps.Add(new SqlParameter("@deno_like", filtros.Buscar));
            }
            var list = new List<string> { "1", "2", "3", "4", "5" };
            if (!string.IsNullOrEmpty(filtros.Tipo) && list.Contains(filtros.Tipo))
            {
                ps.Add(new SqlParameter("@tipo", true));
                ps.Add(new SqlParameter("@tipo_like", filtros.Tipo));
            }

            ps.Add(new SqlParameter("@registros", filtros.Registros));

            List<PlanCuentaDto> res = _repository.EjecutarLstSpExt<PlanCuentaDto>(sp, ps, true);

            //debo analizar el primer registro. si el primer registro no es "Padre"
            //tengo que agregar el registro padre del registro analizado
            if (res.Count == 0)
            {
                return res;
            }
            else
            {
                return BuscaYCargaPadre(sp, ps, res);
            }

        }

        private List<PlanCuentaDto> BuscaYCargaPadre(string sp, List<SqlParameter> ps, List<PlanCuentaDto> res)
        {
            var first = res.First();
            if (first.ccb_id_padre.Equals("00000000"))
            {
                return res;
            }
            else
            {
                //debo buscar al padre del actual registro
                var id = first.ccb_id_padre;
                ps.Clear();
                ps.Add(new SqlParameter("@id", true));
                ps.Add(new SqlParameter("@id_d", id));
                ps.Add(new SqlParameter("@id_h", id));
                var res2 = _repository.EjecutarLstSpExt<PlanCuentaDto>(sp, ps, true);
                if (res2.Count == 0)
                {
                    throw new NegocioException("No se logró encontrar el Padre de la Cuenta inicial solicitada. Si el problema continua avise al Administrador del Sistema.");
                }
                else
                {                    
                    res2.AddRange(res);
                    if(res2.First().ccb_id_padre.Equals("00000000"))
                    {
                        return res2;
                    }
                    else
                    {
                        //busco el padre del padre
                        res2 = BuscaYCargaPadre(sp, ps, res2);
                    }                   
                }
                return res2;
            }
        }
    }
}
