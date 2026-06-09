using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace gc.api.core.Servicios.LineaCaja
{
    public class ApiCajaServicio : Servicio<EntidadBase>, IApiCajaServicio
    {
        private readonly ILoggerHelper _logger;
        public ApiCajaServicio(IUnitOfWork uow, ILoggerHelper logger) : base(uow)
        {
            _logger = logger;
        }

        public RespuestaDto ValidaIntegridadUsuarioCaja(CajaReqDto req)
        {
            _logger.Log(TraceEventType.Information, $"{MethodBase.GetCurrentMethod().Name} - request:{JsonConvert.SerializeObject(req)}");
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
            _logger.Log(TraceEventType.Information, $"{MethodBase.GetCurrentMethod().Name} - request:{JsonConvert.SerializeObject(reqDto)}");
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
            _logger.Log(TraceEventType.Information, $"{MethodBase.GetCurrentMethod().Name} - request:{JsonConvert.SerializeObject(reqDto)}");
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
            _logger.Log(TraceEventType.Information, $"{MethodBase.GetCurrentMethod().Name} - busqueda:{busqueda}, adm_id:{adm_id}, usu_id:{usu_id}");
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
            _logger.Log(TraceEventType.Information, $"{MethodBase.GetCurrentMethod().Name} - origen:{origen}, valor:{valor}, adm_id:{adm_id}, usu_id:{usu_id}");
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

        
        public RespuestaDto ConfirmaConsumidorFinal(ClienteRequestDto req)
        {
            _logger.Log(TraceEventType.Information, $"{MethodBase.GetCurrentMethod().Name} - request:{JsonConvert.SerializeObject(req)}");
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_CF_CONFIRMAR;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@abm", req.Abm),
                new SqlParameter("@tdoc_id", req.TdocId),
                new SqlParameter("@cta_documento", req.CtaDocumento),
                new SqlParameter("@cta_nombre", req.CtaNombre),
                new SqlParameter("@cta_apellido", req.CtaApellido),
                new SqlParameter("@sexo", req.Sexo),
                new SqlParameter("@cta_domicilio", req.CtaDomicilio),
                new SqlParameter("@cta_celu", req.CtaCelu),
                new SqlParameter("@cta_email", req.CtaEmail),
                new SqlParameter("@adm_id", req.AdmId),
                new SqlParameter("@usu_id", req.UsuId)
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
            _logger.Log(TraceEventType.Information, $"{MethodBase.GetCurrentMethod().Name} - caja_id:{caja_id}");
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
            _logger.Log(TraceEventType.Information, $"{MethodBase.GetCurrentMethod().Name} - usu_id:{usu_id}, adm_id:{adm_id}");
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
            _logger.Log(TraceEventType.Information, $"{MethodBase.GetCurrentMethod().Name} - usu_id:{usu_id}, adm_id:{adm_id}");
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
            _logger.Log(TraceEventType.Information, $"{MethodBase.GetCurrentMethod().Name} - adm_id:{adm_id}");
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_CAJA_PV_ABIERTOS;
			var ps = new List<SqlParameter>()
			{
				new("@adm_id",adm_id),
			};
			List<CajaPVAbiertosDto> resp = _repository.EjecutarLstSpExt<CajaPVAbiertosDto>(sp, ps, true);
			return resp;
		}

        public RespuestaDto ValidaEstadoPV(CajaValidaPVDto req)
        {
            _logger.Log(TraceEventType.Information, $"{MethodBase.GetCurrentMethod().Name} - request:{JsonConvert.SerializeObject(req)}");
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_VALIDA_PV;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@caja_id", req.caja_id),
                new SqlParameter("@usu_id", req.usu_id),
                new SqlParameter("@adm_id", req.adm_id),
                new SqlParameter("@caja_nro_proceso", req.caja_nro_proceso),
                new SqlParameter("@caja_nro_cierre", req.caja_nro_cierre),
                new SqlParameter("@tipo_llamada", req.tipo_llamada),
            };

            var res = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps);

            if (res != null && res.Count > 0)
            {
                _logger.Log(TraceEventType.Information, $"{MethodBase.GetCurrentMethod().Name} - Response:{JsonConvert.SerializeObject(res)}");
                return res[0];
            }
            return new() { resultado = -1, resultado_msj = "Hubo un error al validar el PUESTO DE VENTA por lo que no se recepcionó respuesta desde la BD." };
        }

        public RespuestaDto CargaStkDeFactura(CargaStkDto req)
        {
            _logger.Log(TraceEventType.Information, $"{MethodBase.GetCurrentMethod().Name} - request:{JsonConvert.SerializeObject(req)}");
            var sp = ConstantesGC.StoredProcedures.SP_CAJA_STK_CARGA;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@box_id", req.box_id),
                new SqlParameter("@tipo", req.tipo),
                new SqlParameter("@id", req.id)
            };

            var res = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps);

            if (res != null && res.Count > 0)
            {
                return res[0];
            }
            return new() { resultado = -1, resultado_msj = "Hubo un error al intentar cargar el stock de la factura, por lo que no se recepcionó respuesta desde la BD." };
        }
    }
}
