﻿using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class RemitoServicio : Servicio<Remito>, IRemitoServicio
    {
        public RemitoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }
        public List<RemitoTransferidoDto> ObtenerRemitosTransferidos(string admId)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_RTR_Pendientes;
            var ps = new List<SqlParameter>()
            {
                    new("@adm_id",admId),
                    new("@ree_id","%")
            };
            var listaTemp = _repository.EjecutarLstSpExt<RemitoTransferidoDto>(sp, ps, true);
            return listaTemp;
        }
    }
}
