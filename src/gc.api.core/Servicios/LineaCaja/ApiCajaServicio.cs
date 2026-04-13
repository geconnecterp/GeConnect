using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.LineaCaja
{
    public class ApiCajaServicio : Servicio<EntidadBase>, IApiCajaServicio
    {
        public ApiCajaServicio(IUnitOfWork uow) : base(uow)
        {

        }

        public RespuestaDto ValidaIntegridadUsuarioCaja(CajaReqDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_VALIDA_INTEGRIDAD;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@usu_id", req.usu_id),
                new SqlParameter("@caja_id", req.caja_id),
                new SqlParameter("@adm_id", req.adm_id)
            };

            var res = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps);

            if (res != null && res.Count > 0)
            {
                return res[0];
            }
            return new() { resultado = -1, resultado_msj = "Hubo un error al validar la integridad del usuario en la caja." };
        }

        public RespuestaDto AperturaCaja(CajaReqDto reqDto)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_APERTURA;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@usu_id", reqDto.usu_id),
                new SqlParameter("@caja_id", reqDto.caja_id),
                new SqlParameter("@adm_id", reqDto.adm_id)
            };
            var res = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps);
            if (res != null && res.Count > 0)
            {
                return res[0];
            }
            return new() { resultado = -1, resultado_msj = "Hubo un error al aperturar la caja." };
        }

        public RespuestaDto CierreCaja(CajaReqDto reqDto)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_CIERRE;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@usu_id", reqDto.usu_id),
                new SqlParameter("@caja_id", reqDto.caja_id),
                new SqlParameter("@adm_id", reqDto.adm_id),
                new SqlParameter("@json_rendiciones", reqDto.json)
            };
            var res = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps);
            if (res != null && res.Count > 0)
            {
                return res[0];
            }
            return new() { resultado = -1, resultado_msj = "Hubo un error al cerrar la caja." };
        }

        public CuentaBusquedaResultadoDto BusquedaCaja_b_cuenta(string busqueda)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_BCUENTA;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@busqueda", busqueda)
            };
            var res = _repository.EjecutarLstSpExt<CuentaBusquedaResultadoDto>(sp, ps);
            if (res != null && res.Count > 0)
            {
                return res[0];
            }
            return new CuentaBusquedaResultadoDto();
        }

        public ProductoDatosResponseDto ObtenerProductoDatos(ProductoDatosRequestDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_BPROD_D;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@tipo_valor", req.tipo_valor),
                new SqlParameter("@valor", req.valor),
                new SqlParameter("@lp_id", req.lp_id),
                new SqlParameter("@adm_id", req.adm_id),
                new SqlParameter("@cantidad", req.cantidad),
                new SqlParameter("@bulto", req.bulto),
                new SqlParameter("@ctc_id", req.ctc_id),
                new SqlParameter("@cta_id", req.cta_id),
                new SqlParameter("@ctac_dto", req.ctac_dto)
            };
            var res = _repository.EjecutarLstSpExt<ProductoDatosResponseDto>(sp, ps);
            if (res != null && res.Count > 0)
            {
                return res[0];
            }
            return new ProductoDatosResponseDto() { respuesta = -1, respuesta_msj = "Hubo un error al obtener los datos del producto." };
        }


        public RespuestaDto Cargar_CF(CargaCFRequestDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_CF_CARGA;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@tdco_id", req.tdco_id),
                new SqlParameter("@documento", req.documento),
                new SqlParameter("@nombre", req.nombre),
                new SqlParameter("@apellido", req.apellido),
                new SqlParameter("@sexo", req.sexo),
                new SqlParameter("@domicilio", req.domicilio),
                new SqlParameter("@celu", req.celu),
                new SqlParameter("@email", req.email),
                new SqlParameter("@adm_id", req.adm_id),
                new SqlParameter("@usu_id", req.usu_id)
            };
            var res = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps);
            if (res != null && res.Count > 0)
            {
                return res[0];
            }
            return new() { resultado = -1, resultado_msj = "Hubo un error al cargar el cliente final." };
        }

        public CajaDatosDto ObtenerDatosCF(string caja_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_DATOS;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@caja_id", caja_id)
            };
            var res = _repository.EjecutarLstSpExt<CajaDatosDto>(sp, ps);
            if (res != null && res.Count > 0)
            {
                return res[0];
            }
            return new CajaDatosDto() { caja_id = caja_id };
        }

        public RespuestaDto CierreCajaGral(string usu_id, string adm_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_GRAL_CIERRE;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@usu_id", usu_id),
                new SqlParameter("@adm_id", adm_id)
            };
            var res = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps);
            if (res != null && res.Count > 0)
            {
                return res[0];
            }
            return new() { resultado = -1, resultado_msj = "Hubo un error al validar el cierre de caja." };
        }

        public RespuestaDto HabilitarCajaGral(string usu_id, string adm_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_GRAL_HAB;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@usu_id", usu_id),
                new SqlParameter("@adm_id", adm_id)
            };
            var res = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps);
            if (res != null && res.Count > 0)
            {
                return res[0];
            }
            return new() { resultado = -1, resultado_msj = "Hubo un error al habilitar la caja general." };
        }

		public List<CajaPVAbiertosDto> ObtenerPVAbiertos(string adm_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_CAJA_PV_ABIERTOS;
			var ps = new List<SqlParameter>()
			{
				new("@adm_id",adm_id),
			};
			List<CajaPVAbiertosDto> resp = _repository.EjecutarLstSpExt<CajaPVAbiertosDto>(sp, ps, true);
			return resp;
		}
	}
}
