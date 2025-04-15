using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.ABM;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Consultas;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace gc.api.core.Servicios.ABM
{
    public class ABMVendedorServicio : Servicio<Vendedor>, IABMVendedorServicio
    {
        public ABMVendedorServicio(IUnitOfWork uow) : base(uow)
        {

        }
       
        public List<ABMVendedorDto> ObtenerVendedores(QueryFilters filtros)
        {
            var sp = ConstantesGC.StoredProcedures.SP_ABM_VENDEDOR_LISTA;

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

            List<ABMVendedorDto> res = _repository.EjecutarLstSpExt<ABMVendedorDto>(sp, ps, true);
            return res;
        }

        public List<ABMVendedorDatoDto> ObtenerVendedorPorId(string ve_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_ABM_VENDEDOR_DATO;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@ve_id",ve_id) ,

            };

            List<ABMVendedorDatoDto> res = _repository.EjecutarLstSpExt<ABMVendedorDatoDto>(sp, ps, true);
            return res;
        }
    }
}
