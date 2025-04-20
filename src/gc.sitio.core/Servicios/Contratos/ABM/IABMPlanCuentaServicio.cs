using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Contabilidad;
using gc.infraestructura.Dtos.Gen;
using Org.BouncyCastle.Asn1.Cms;

namespace gc.sitio.core.Servicios.Contratos.ABM
{
	public interface IABMPlanCuentaServicio : IServicio<PlanCuentaDto>
	{
        Task<RespuestaGenerica<PlanCuentaDto>> ObtenerCuentaPorId(string id, string tokenCookie);
        Task<(List<PlanCuentaDto>,MetaData?)> ObtenerPlanCuentas(QueryFilters filters, string token);
	}
}
