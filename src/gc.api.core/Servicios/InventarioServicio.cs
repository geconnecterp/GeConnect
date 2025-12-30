using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Dtos.Inventario.Dto;
using gc.infraestructura.Dtos.Inventario.Request;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.api.core.Servicios
{
    public class InventarioServicio : Servicio<Inventario>, IInventarioServicio
    {
        public InventarioServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }

        public List<InventarioListaDto> GetInventarioLista(GetInventarioListaRequest request)
        {
            var sp = ConstantesGC.StoredProcedures.SP_INV_LISTA;
            var d = request.desde.ToString("yyyy/MM/dd");
            var h = request.hasta.ToString("yyyy/MM/dd");
            var ps = new List<SqlParameter>()
            {
                new("@desde",d),
                new("@hasta",h),
                new("@adm_id",request.adm_id),
                new("@usu_id",request.usu_id),
                new("@inve_id",request.inve_id),
            };
            var listaTemp = _repository.EjecutarLstSpExt<InventarioListaDto>(sp, ps, true);
            return listaTemp;
        }

        public List<RubroEnInventarioDto> GetRubrosEnInventario(string inv_nro, string usu_id = "%")
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_RUBROS;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",inv_nro),
                new("@usu_id",usu_id),
            };
            var listaTemp = _repository.EjecutarLstSpExt<RubroEnInventarioDto>(sp, ps, true);
            return listaTemp;
        }

        public List<UsuarioEnInventarioDto> GetUSuariosEnInventario(string inv_nro)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_USUARIOS;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",inv_nro),
            };
            var listaTemp = _repository.EjecutarLstSpExt<UsuarioEnInventarioDto>(sp, ps, true);
            return listaTemp;
        }

        public List<RespuestaDto> ConfirmarInventario(ConfirmarInventarioRequest request)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_CONFIRMAR;
            var ps = new List<SqlParameter>()
            {
                new("@abm",request.abm),
                new("@inv_nro",request.inv_nro),
                new("@invt_id",request.invt_id),
                new("@inv_descripcion",request.inv_descripcion),
                new("@inv_apertura",request.inv_apertura),
                new("@inv_cierre",request.inv_cierre),
                new("@depo_id",request.depo_id),
                new("@adm_id",request.adm_id),
                new("@usu_id",request.usu_id),
                new("@json_r",request.json_r),
                new("@json_u",request.json_u),
            };
            var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            return listaTemp;
        }



        public List<InventarioListaDto> GetInventarioDatos(GetInventarioDatosRequest request)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_DATOS;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",request.inv_nro),
            };
            var listaTemp = _repository.EjecutarLstSpExt<InventarioListaDto>(sp, ps, true);
            return listaTemp;
        }

        public List<RespuestaDto> RegistrarControlDeStock(RegistrarStockDeControlRequest request)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_REGISTRA_CTRL_STK;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",request.inv_nro),
                new("@adm_id",request.adm_id),
                new("@usu_id",request.usu_id),
            };
            var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            return listaTemp;
        }

        public List<ProductosEnValorizacionDto> GetProductosEnValorizacion(ProductosEnValorizacionRequest request)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_PRODUCTOS;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",request.inv_nro),
                new("@tipo",request.tipo),
                new("@tipo_id",request.tipo_id),
                new("@usu_id",request.usu_id),
            };
            var listaTemp = _repository.EjecutarLstSpExt<ProductosEnValorizacionDto>(sp, ps, true);
            return listaTemp;
        }

        public List<ConteoEnValorizacionDto> GetConteoEnValorizacion(ConteosEnValorizacionRequest request)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_CONTEOS;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",request.inv_nro),
                new("@tipo",request.tipo),
                new("@tipo_id",request.tipo_id),
                new("@usu_id",request.usu_id),
                new("@p_id",request.p_id),
            };
            var listaTemp = _repository.EjecutarLstSpExt<ConteoEnValorizacionDto>(sp, ps, true);
            return listaTemp;
        }

        public List<InventarioBoxDto> GetInventarioBox(string inv_nro, string usu_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_INV_BOX;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",inv_nro),
                new("@usu_id",usu_id),
            };

            var res = _repository.EjecutarLstSpExt<InventarioBoxDto>(sp, ps, true);
            return res;
        }

        public List<InventarioPlanillaDto> GetInventarioPlanilla(string inv_nro, string usu_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_INV_PLANILLA;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",inv_nro),
                new("@usu_id",usu_id),
            };
            var listaTemp = _repository.EjecutarLstSpExt<InventarioPlanillaDto>(sp, ps, true);
            return listaTemp;
        }

        public RespuestaDto ValidarConteo(InventarioRequestDto request)
        {
            var sp = ConstantesGC.StoredProcedures.SP_INV_CONTEO_VALIDA;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",request.inv_nro),
                new("@usu_id",request.usu_id),
                new("@tipo",request.tipo),
                new("@tipo_id",request.tipo_id),
            };
            var resultado = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (resultado != null && resultado.Count > 0)
            {
                return resultado[0];
            }
            else
            {
                return new RespuestaDto
                {
                    resultado = -1,
                    resultado_msj = "No se obtuvo respuesta del procedimiento almacenado."
                };
            }
        }

        public List<InventarioConteoDto> GetInventarioConteo(InventarioRequestDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_INV_CONTEOS;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",req.inv_nro),
                new("@usu_id",req.usu_id),
                new("@tipo",req.tipo),
                new("@tipo_id",req.tipo_id),
                new("@p_id",req.p_id),
            };
            var resultado = _repository.EjecutarLstSpExt<InventarioConteoDto>(sp, ps, true);
            return resultado;
        }

        public RespuestaDto InventarioConfirmarConteo(InventarioRequestDto request)
        {
            var sp = ConstantesGC.StoredProcedures.SP_INV_CONTEO_CONFIRMA;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",request.inv_nro),
                new("@usu_id",request.usu_id),
                new("@tipo",request.tipo),
                new("@tipo_id",request.tipo_id),
                new("@json_p", request.json_p),
            };
            var resultado = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (resultado != null && resultado.Count > 0)
            {
                return resultado[0];
            }
            else
            {
                return new RespuestaDto
                {
                    resultado = -1,
                    resultado_msj = "No se obtuvo respuesta de la confirmación del conteo."
                };
            }
        }

		public List<RespuestaDto> RegistrarValorizacion(RegistrarValorizacionRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_VALORIZA;
			var ps = new List<SqlParameter>()
			{
				new("@inv_nro",request.inv_nro),
				new("@adm_id",request.adm_id),
				new("@usu_id",request.usu_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<ProductoEnCierreDto> GetProductosEnCierre(ProductosEnCierreRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_PRODUCTOS;
			var ps = new List<SqlParameter>()
			{
				new("@inv_nro",request.inv_nro),
				new("@tipo",request.tipo),
				new("@tipo_id",request.tipo_id),
				new("@usu_id",request.usu_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<ProductoEnCierreDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> RegistrarCierre(RegistrarCierreRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_CERRAR;
			var ps = new List<SqlParameter>()
			{
				new("@inv_nro",request.inv_nro),
				new("@adm_id",request.adm_id),
				new("@usu_id",request.usu_id),
				new("@json_p",request.json_p),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

        public List<InvRepoStkVsConteoDto> GetReporteStockVsConteo(ReporteInventarioRequest request)
        {
            var sp = ConstantesGC.StoredProcedures.SP_INV_REPO_STK_VS_CONTEO;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",request.inv_nro),
            };
            var listaTemp = _repository.EjecutarLstSpExt<InvRepoStkVsConteoDto>(sp, ps, true);
            return listaTemp;
		}

        public List<InvRepoValPorSecDto> GetReporteValorizacionPorSector(ReporteInventarioRequest request)
        {
            var sp = ConstantesGC.StoredProcedures.SP_INV_REPO_VAL_X_SEC;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",request.inv_nro),
            };
            var listaTemp = _repository.EjecutarLstSpExt<InvRepoValPorSecDto>(sp, ps, true);
            return listaTemp;
		}

        public List<InvRepoValPorRubDto> GetReporteValorizacionPorRubro(ReporteInventarioRequest request)
        {
            var sp = ConstantesGC.StoredProcedures.SP_INV_REPO_VAL_X_RUB;
            var ps = new List<SqlParameter>()
            {
                new("@inv_nro",request.inv_nro),
            };
            var listaTemp = _repository.EjecutarLstSpExt<InvRepoValPorRubDto>(sp, ps, true);
            return listaTemp;
		}

        public List<InvRepoValorDetalleDto> GetReporteValorizadoDetalle(ReporteInventarioRequest request)
        {
			var sp = ConstantesGC.StoredProcedures.SP_INV_REPO_VAL_DETALLE;
			var ps = new List<SqlParameter>()
			{
				new("@inv_nro",request.inv_nro),
			};
			var listaTemp = _repository.EjecutarLstSpExt<InvRepoValorDetalleDto>(sp, ps, true);
			return listaTemp;
		}

        public List<InvRepoConteosPorUsuDto> GetReporteConteosPorUsu(ReporteInventarioRequest request)
        {
			var sp = ConstantesGC.StoredProcedures.SP_INV_REPO_CONTEO_X_USU;
			var ps = new List<SqlParameter>()
			{
				new("@inv_nro",request.inv_nro),
				new("@usu_id",request.usu_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<InvRepoConteosPorUsuDto>(sp, ps, true);
			return listaTemp;
		}
	}
}
