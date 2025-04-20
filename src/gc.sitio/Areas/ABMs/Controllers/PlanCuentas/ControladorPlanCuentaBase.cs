using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Contabilidad;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers.PlanCuenta
{
    public class ControladorPlanCuentaBase : ControladorBase
    {
        private readonly AppSettings _setting;
        public ControladorPlanCuentaBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
        {
            _setting = options.Value;
        }

        public List<PlanCuentaDto> PlanCuentasLista
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("PlanCuentasLista");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<PlanCuentaDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("PlanCuentasLista", json);
            }
        }

        protected PlanCuentaDto CuentaSeleccionada
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("CuentaSeleccionada");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return new PlanCuentaDto();
                }
                if(!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<PlanCuentaDto>(json);
                }
                else
                {
                    return new PlanCuentaDto();
                }
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("CuentaSeleccionada", json);
            }
        }
    }
}
