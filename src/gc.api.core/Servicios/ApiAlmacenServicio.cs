﻿using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
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
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_BOX_ALMACENA_UL;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@box_id",req.Box),
                new SqlParameter("@ul_id",req.UL),
                new SqlParameter("@adm_id",req.AdmId)
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
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_VALIDAR_BOX;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@box_id",box),
                new SqlParameter("@adm_id",admid),

            };

            List<RprResponseDto> response = _repository.EjecutarLstSpExt<RprResponseDto>(sp, ps, true);

            return response[0];
        }

        public RprResponseDto ValidarUL(string ul, string admid)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_VALIDAR_UL;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@ul_id",ul),
                new SqlParameter("@adm_id",admid),

            };

            List<RprResponseDto> response = _repository.EjecutarLstSpExt<RprResponseDto>(sp, ps, true);

            return response[0];
        }

        
    }
}
