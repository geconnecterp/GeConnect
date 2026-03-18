using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Interfaces.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Gen;
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


        public List<ORListaDto> ObtenerListaORbyRubro(string or_compte,string adm,string usu)
        {
            var sp = ConstantesGC.StoredProcedures.SP_OR_LISTA_RUBROS;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@or_compte", or_compte),
                new SqlParameter("@adm_id", adm),
                new SqlParameter("@usu_id", usu)
             }; 

            var result = _repository.EjecutarLstSpExt<ORListaDto>(sp, ps, true);
            return result;
        }

        public List<ORListaDto> ObtenerListaORbyBox(string or_compte, string adm, string usu)
        {
            var sp = ConstantesGC.StoredProcedures.SP_OR_LISTA_BOX;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@or_compte", or_compte),
                new SqlParameter("@adm_id", adm),
                new SqlParameter("@usu_id", usu)
             };

            var result = _repository.EjecutarLstSpExt<ORListaDto>(sp, ps, true);
            return result;
        }


        public List<ORProductoDto> ObtenerListaORProductos(ORProdRequestDto request)
        {
            var sp = ConstantesGC.StoredProcedures.SP_OR_LISTA_PRODUCTOS;
            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@or_compte", request.or_compte),
                new SqlParameter("@adm_id", request.adm_id),
                new SqlParameter("@usu_id", request.usu_id),
                new SqlParameter("@box_id", request.box_id),
                new SqlParameter("@rub_id", request.rub_id)
            };
            var result = _repository.EjecutarLstSpExt<ORProductoDto>(sp, ps, true);
            return result;

        }

        public List<OrCtlProductoDto> ObtenerListaProductosOrCtl(string or_compte,string usu_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_OR_VER_CTL_SALIDA;
            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@or_compte", or_compte),
                new SqlParameter("@usu_id", usu_id),
            };
            var result = _repository.EjecutarLstSpExt<OrCtlProductoDto>(sp, ps, true);
            return result;
        }

        public RespuestaDto CargaProductoORCtl(string json)
        {
            var sp = ConstantesGC.StoredProcedures.SP_OR_CARGA_CTL_SALIDA;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@json", json),
            };
               
            var result = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (result != null && result.Count > 0)
            {
                return result[0];
            }
            else
            {
                return new RespuestaDto()
                {
                    resultado = -1,
                    resultado_msj = "Error al cargar el producto"
                };
            }
        }

        public RespuestaDto ValidaProductoCarritoOR(ORCargaCarritoRequest request)
        {
            var sp = ConstantesGC.StoredProcedures.SP_OR_CARRITO_VALIDA;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@or_compte", request.or_compte),
                new SqlParameter("@adm_id", request.adm_id),
                new SqlParameter("@usu_id", request.usu_id),
                new SqlParameter("@box_id", request.box_id),
                new SqlParameter("@desarma_box", request.desarma_box),
                new SqlParameter("@p_id", request.p_id),
                new SqlParameter("@unidad_pres", request.unidad_pres),
                new SqlParameter("@bulto", request.bulto),
                new SqlParameter("@us", request.us),
                new SqlParameter("@cantidad", request.cantidad),
                new SqlParameter("@fv", request.fv)
            };
            var result = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if(result != null && result.Count > 0)
            {
                return result[0];
            }
            else
            {
                return new RespuestaDto()
                {
                    resultado = -1,
                    resultado_msj = "Error al validar el producto"
                };
            }
        }

        public RespuestaDto ResguardarProductoCarrito(ORCargaCarritoRequest request)
        {
            var sp = ConstantesGC.StoredProcedures.SP_OR_CARRITO_CARGA;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@or_compte", request.or_compte),
                new SqlParameter("@adm_id", request.adm_id),
                new SqlParameter("@usu_id", request.usu_id),
                new SqlParameter("@box_id", request.box_id),
                new SqlParameter("@desarma_box", request.desarma_box),
                new SqlParameter("@p_id", request.p_id),
                new SqlParameter("@unidad_pres", request.unidad_pres),
                new SqlParameter("@bulto", request.bulto),
                new SqlParameter("@us", request.us),
                new SqlParameter("@cantidad", request.cantidad),
                new SqlParameter("@fv", request.fv)
            };
            var result = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (result != null && result.Count > 0)
            {
                return result[0];
            }
            else
            {
                return new RespuestaDto()
                {
                    resultado = -1,
                    resultado_msj = "Error al cargar el producto"
                };
            }
        }



    }
}
