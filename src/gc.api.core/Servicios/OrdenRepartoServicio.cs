using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Interfaces.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.OrdenReparto;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Servicios
{
    public class OrdenRepartoServicio : Servicio<EntidadBase>, IOrdenRepartoServicio
    {
        public OrdenRepartoServicio(IUnitOfWork uow):base(uow)
        {
            
        }

        public List<OrdenRepartoListDto> ObtenerOrdenesReparto(ORRequestDto request)
        {
            var sp = ConstantesGC.StoredProcedures.SP_OR_LISTA;
            var ps = new List<SqlParameter>();
            //ANALIZAMOS LOS PARAMETROS
            if (request.HasFecha)
            {
                ps.Add(new SqlParameter("@f", true));
                ps.Add(new SqlParameter("@desde", request.Desde));
                ps.Add(new SqlParameter("@hasta", request.Hasta));
            }
            else
            {
                ps.Add(new SqlParameter("@f", false));
                ps.Add(new SqlParameter("@desde", DBNull.Value));
                ps.Add(new SqlParameter("@hasta", DBNull.Value));
            }

            if (request.HasEstado)
            {
                ps.Add(new SqlParameter("@e", true));
                ps.Add(new SqlParameter("@ore_list", request.Ore_list));
            }
            else
            {
                ps.Add(new SqlParameter("@e", false));
                ps.Add(new SqlParameter("@ore_list", DBNull.Value));
            }

            if(request.HasRepartidor)
            {
                ps.Add(new SqlParameter("@r", true));
                ps.Add(new SqlParameter("@rp_list", request.RP_List));
            }
            else
            {
                ps.Add(new SqlParameter("@r", false));
                ps.Add(new SqlParameter("@rp_list", DBNull.Value));
            }

            if(request.HasId)
            {
                ps.Add(new SqlParameter("@id", true));
                ps.Add(new SqlParameter("@or_compte", request.OR_Compte));
            }
            else
            {
                ps.Add(new SqlParameter("@id", false));
                ps.Add(new SqlParameter("@or_compte", DBNull.Value));
            }

            var result = _repository.EjecutarLstSpExt<OrdenRepartoListDto>(sp, ps,true);
            return result;

        }


        public OrdenRepartoDto ObtenerOrdenRepartoPorId(int id)
        {
            throw new NotImplementedException();
        }
    }
}
