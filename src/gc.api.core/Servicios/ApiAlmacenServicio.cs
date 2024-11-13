using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Deposito;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios
{
    public class ApiAlmacenServicio : Servicio<Producto>, IApiAlmacenServicio
    {
        public ApiAlmacenServicio(IUnitOfWork uow) : base(uow)
        {

        }

        public RprResponseDto AlmacenaBoxUl(RprABRequest req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_BOX_ALMACENA_UL;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@box_id",req.Box),
                new SqlParameter("@ul_id",req.UL),
                new SqlParameter("@adm_id",req.AdmId),
                new SqlParameter("@sm_tipo",req.Sm),

            };

            List<RprResponseDto> response = _repository.EjecutarLstSpExt<RprResponseDto>(sp, ps, true);

            return response[0];
        }

        public List<AutorizacionTIDto> TRObtenerAutorizacionesPendientes(string admId, string usuId, string titId)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_AUTORIZACIONES_PENDIENTES;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@adm_id",admId),
                new SqlParameter("@usu_id",usuId),
                new SqlParameter("@tit_id",titId),

            };

            List<AutorizacionTIDto> response = _repository.EjecutarLstSpExt<AutorizacionTIDto>(sp, ps, true);

            return response;
        }

        public RprResponseDto ValidarBox(string box, string admid)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_VALIDAR_BOX;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@box_id",box),
                new SqlParameter("@adm_id",admid),

            };

            List<RprResponseDto> response = _repository.EjecutarLstSpExt<RprResponseDto>(sp, ps, true);

            return response[0];
        }

        public RespuestaDto ValidarUL(string ul, string admid,string sm)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_VALIDAR_UL;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@ul_id",ul),
                new SqlParameter("@adm_id",admid),
                new SqlParameter("@sm_tipo",sm),
            };

            List<RespuestaDto> response = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);

            return response[0];
        }

        public List<BoxRubProductoDto> TIObtenerListaRubro(string admId, string usuId, string ti)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Lista_Rubros;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@adm_id",admId),
                new SqlParameter("@usu_id",usuId),
                new SqlParameter("@ti",ti),

            };

            List<BoxRubProductoDto> response = _repository.EjecutarLstSpExt<BoxRubProductoDto>(sp, ps, true);

            return response;
        }
        public List<BoxRubProductoDto> TIObtenerListaBox(string admId, string usuId, string ti)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Lista_BOX;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@adm_id",admId),
                new SqlParameter("@usu_id",usuId),
                new SqlParameter("@ti",ti),

            };

            List<BoxRubProductoDto> response = _repository.EjecutarLstSpExt<BoxRubProductoDto>(sp, ps, true);

            return response;
        }

        public List<TiListaProductoDto> BuscaTIListaProductos(string admId, string usuId, string ti, string boxid, string rubroid)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_Lista_Productos;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@ti",ti),
                new SqlParameter("@adm_id",admId),
                new SqlParameter("@usu_id",usuId),
                new SqlParameter("@box_id",boxid),
                new SqlParameter("@rub_id",rubroid),

            };

            List<TiListaProductoDto> response = _repository.EjecutarLstSpExt<TiListaProductoDto>(sp, ps, true);

            return response;
        }

		public List<DepositoInfoBoxDto> ObtenerListaDeBoxesPorDeposito(string depoId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_BOX_LISTA;

			var ps = new List<SqlParameter>()
			{
				new("@depo_id",depoId),
			};

			List<DepositoInfoBoxDto> response = _repository.EjecutarLstSpExt<DepositoInfoBoxDto>(sp, ps, true);

			return response;
		}

		public List<DepositoInfoBoxDto> ObtenerInfoDeBox(string boxId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_BOX_INFO;

			var ps = new List<SqlParameter>()
			{
				new("@box_id",boxId),
			};

			List<DepositoInfoBoxDto> response = _repository.EjecutarLstSpExt<DepositoInfoBoxDto>(sp, ps, true);

			return response;
		}
	}
}
