using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Contabilidad;

namespace gc.api.core.Contratos.Servicios.ABM
{
    public interface IABMPlanCuentaServicio:IServicio<PlanContable>
    {
        List<PlanCuentaDto> ObtenerPlanCuenta(QueryFilters filters);    
        PlanCuentaDto ObtenerCuenta(string ccb_id);
    }
}
