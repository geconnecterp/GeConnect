using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Request;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class FinancieroServicio : Servicio<Financiero>, IFinancieroServicio
    {
        public FinancieroServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

		public List<PlanContableDto> GetPlanContableCuentaLista()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_CCB_CUENTA_LISTA;
			var ps = new List<SqlParameter>();
			var listaTemp = _repository.EjecutarLstSpExt<PlanContableDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroEstadoDto> GetFinancieroEstados()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_FINANCIERO_ESTADOS;
			var ps = new List<SqlParameter>();
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroEstadoDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroDto> GetFinancierosPorTipoCfLista(string tcf_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_FINANCIEROS_LISTA;
            var ps = new List<SqlParameter>()
            {
                new("@tcf_id",tcf_id)
            };
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new FinancieroDto()
                {
                    #region Campos
                    ctaf_id=x.Ctaf_id,
                    ctaf_denominacion = x.Ctaf_denominacion,
                    ctaf_activo = x.Ctaf_activo,
                    ctaf_lista = x.Ctaf_lista,
                    #endregion
                }).ToList();
        }

		public List<FinancieroDto> GetFinancierosRelaPorTipoCfLista(string tcf_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_FINANCIEROS_RELA_LISTA;
			var ps = new List<SqlParameter>()
			{
				new("@tcf_id",tcf_id)
			};
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new FinancieroDto()
				{
					#region Campos
					ctaf_id = x.Ctaf_id,
					ctaf_denominacion = x.Ctaf_denominacion,
					ctaf_activo = x.Ctaf_activo,
					ctaf_lista = x.Ctaf_lista,
					#endregion
				}).ToList();
		}

		public List<FinancieroDesdeSeleccionDeTipoDto> GetFinancieroDesdeTipoParaSeleccionDeValores(string tcf_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OP_SV_CTAF;
			var ps = new List<SqlParameter>()
			{
				new("@tcf_id",tcf_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroDesdeSeleccionDeTipoDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroCarteraDto> GetFinancieroCarteraParaSeleccionDeValores(string ctaf_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OP_SV_CARTERA;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",ctaf_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroCarteraDto>(sp, ps, true);
			return listaTemp;
		}
	}
}
