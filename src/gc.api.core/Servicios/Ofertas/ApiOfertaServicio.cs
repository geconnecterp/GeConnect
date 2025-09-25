using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace gc.api.core.Servicios.Ofertas
{
    public class ApiOfertaServicio : Servicio<EntidadBase>, IApiOfertaServicio
    {
        public ApiOfertaServicio(IUnitOfWork uow) : base(uow)
        {

        }

        public List<CanalDto> BuscarCanales()
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_CANALES_LIST;

            var ps = new List<SqlParameter>();

            List<CanalDto> canales = _repository.EjecutarLstSpExt<CanalDto>(sp, ps);
            return canales;
        }

        public string ConocerEstadoOferta(string p_id, string admId, string lp_id)
        {
            var fx = $"select {ConstantesGC.StoredFunctions.FX_PROD_OFERTA}('{p_id}','{admId}','{lp_id}')";
            string estado = _repository.EjecutarFunctionScalar<string>(fx);
            return estado;
        }

        public RespuestaDto ConfirmacionAltaOferta(AbmPlusGenDto req, ParamOferta param)
        {
            string sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_CARGA;

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@oferta", param.Precio),
                new SqlParameter("@desde", param.Desde),
                new SqlParameter("@hasta", param.Hasta),
                new SqlParameter("@tope", param.TopeVta),
                new SqlParameter("@json_p", req.Json),
                new SqlParameter("@json_a", req.Json2),
                new SqlParameter("@usu_id", req.Usuario),
                new SqlParameter("@adm_id", req.Administracion),
            };


            List<RespuestaDto> resultado = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (resultado != null && resultado.Count > 0)
            {
                return resultado[0];
            }
            return new() { resultado = -1, resultado_msj = "No se logro obtener el resultado del proceso. " };

        }

        public List<OfertaEstadoDto> ObtenerEstadoOfertaProducto(string p_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_ESTADO;
            var ps = new List<SqlParameter>
            {
                new SqlParameter("@p_id", p_id)
            };
            List<OfertaEstadoDto> estados = _repository.EjecutarLstSpExt<OfertaEstadoDto>(sp, ps, true);
            return estados;
        }

        public List<OfertaDto> ObtenerOfertas(string admId, string lp_id, bool sinActivar = true)
        {
            string sp;
            //depeniendo del flag trae las ofertas activas o las sin activar
            if (sinActivar)
                sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_SIN_ACTIVAR;
            else
                sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_ACTIVA;

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@adm_id_ofe", admId),
                new SqlParameter("@lp_id_ofe", lp_id)
            };
            List<OfertaDto> ofertas = _repository.EjecutarLstSpExt<OfertaDto>(sp, ps, true);
            return ofertas;
        }

        public RespuestaDto EliminarOfertas(AbmPlusGenDto req)
        {
            //la logica es identica se reutiliza el metodo para que funcione como eliminar oferta
            return ActivacionDeOferta(req, true);
        }

        public RespuestaDto EliminaOfertasActivas(AbmGenDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_ELIMINA_ACTIVA;
            var obj = req.Objeto.Split('#', StringSplitOptions.RemoveEmptyEntries);

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@adm_id_ofe", obj[0]),
                new SqlParameter("@lp_id_ofe", obj[1]),
                new SqlParameter("@json_p", req.Json),
                new SqlParameter("@usu_id", req.Usuario),
                new SqlParameter("@adm_id", req.Administracion),
            };

            List<RespuestaDto> resultado = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (resultado != null && resultado.Count > 0)
            {
                return resultado[0];
            }
            return new()
            {
                resultado = -1,
                resultado_msj = "No se logro obtener el resultado del proceso. "
            };
        }

        public RespuestaDto CopiarACanal(AbmPlusGenDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_COPIAR_A;
            var obj = req.Objeto.Split('#', StringSplitOptions.RemoveEmptyEntries);

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@adm_id_ofe", obj[0]),
                new SqlParameter("@lp_id_ofe", obj[1]),
                new SqlParameter("@json_p", req.Json),
                new SqlParameter("@json_destino", req.Json2),
                new SqlParameter("@usu_id", req.Usuario),
                new SqlParameter("@adm_id", req.Administracion),
            };

            List<RespuestaDto> resultado = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (resultado != null && resultado.Count > 0)
            {
                return resultado[0];
            }
            return new()
            {
                resultado = -1,
                resultado_msj = "No se logro obtener el resultado del proceso. "
            };
        }

        public RespuestaDto ActivacionDeOferta(AbmPlusGenDto req, bool eliminar = false)
        {
            string sp;
            if (!eliminar)
            {
                sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_ACTIVAR;
            }
            else
            {
                sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_ELIMINA_A_SINACT;
            }

            //trae separado por #, el id de la administracion y el id de la lista de precios "0000#001"
            var obj = req.Objeto.Split('#', StringSplitOptions.RemoveEmptyEntries);

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@adm_id_ofe", obj[0]),
                new SqlParameter("@lp_id_ofe", obj[1]),
                new SqlParameter("@json_p", req.Json),
                new SqlParameter("@usu_id", req.Usuario),
                new SqlParameter("@adm_id", req.Administracion),
            };
            if (!eliminar)
            {
                ps.Add(new SqlParameter("@elimina", eliminar));
            }

            List<RespuestaDto> resultado = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (resultado != null && resultado.Count > 0)
            {
                return resultado[0];
            }
            return new()
            {
                resultado = -1,
                resultado_msj = "No se logro obtener el resultado del proceso. "
            };
        }

        public RespuestaDto ActualizarOfertaVencidaSinActivar(AbmGenDto req)
        {
            string sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_ACTU_VTO_SINACT;
            var obj = req.Objeto.Split('#', StringSplitOptions.RemoveEmptyEntries);

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@adm_id_ofe", obj[0]),
                new SqlParameter("@lp_id_ofe", obj[1]),
                new SqlParameter("@usu_id", req.Usuario),
                new SqlParameter("@adm_id", req.Administracion),
            };

            List<RespuestaDto> resultado = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (resultado != null && resultado.Count > 0)
            {
                return resultado[0];
            }
            return new()
            {
                resultado = -1,
                resultado_msj = "No se logro obtener el resultado del proceso. "
            };
        }

        public RespuestaDto CargarActivasASinActivar(AbmGenDto req)
        {
            string sp = ConstantesGC.StoredProcedures.SP_PROD_OFERTA_ACTIVAS_A_SINACT;
            var obj = req.Objeto.Split('#', StringSplitOptions.RemoveEmptyEntries);

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@adm_id_ofe", obj[0]),
                new SqlParameter("@lp_id_ofe", obj[1]),
                new SqlParameter("@usu_id", req.Usuario),
                new SqlParameter("@adm_id", req.Administracion),
            };

            List<RespuestaDto> resultado = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (resultado != null && resultado.Count > 0)
            {
                return resultado[0];
            }
            return new()
            {
                resultado = -1,
                resultado_msj = "No se logro obtener el resultado del proceso. "
            };
        }


    }
}

