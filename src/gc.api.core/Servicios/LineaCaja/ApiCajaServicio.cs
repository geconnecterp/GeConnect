using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
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

        /// <summary>
        /// esta metodo puede devolver 0,1 o mas registros 
        /// </summary>
        /// <param name="busqueda">criterio de busqueda</param>
        /// <param name="adm_id">sucursal</param>
        /// <param name="usu_id">usuario</param>
        /// <returns></returns>
        public List<CuentaBusquedaResultadoDto> BusquedaClientes(string busqueda, string adm_id, string usu_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_BCUENTA;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@busqueda", busqueda),
                new SqlParameter("@adm_id", adm_id),
                new SqlParameter("@usu_id", usu_id)
            };
            var res = _repository.EjecutarLstSpExt<CuentaBusquedaResultadoDto>(sp, ps, true);

            return res;
        }

        public CuentaDatosResultadoDto BusquedaDatosCliente(string origen, string valor, string adm_id, string usu_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_BCUENTA_D;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@origen", origen),
                new SqlParameter("@valor", valor),
                new SqlParameter("@adm_id", adm_id),
                new SqlParameter("@usu_id", usu_id)
            };
            var res = _repository.EjecutarLstSpExt<CuentaDatosResultadoDto>(sp, ps, true);
            if (res == null || !res.Any())
            {
                throw new NegocioException($"No se logró recuperar los datos de {origen}-{valor}");
            }

            return res[0];
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
