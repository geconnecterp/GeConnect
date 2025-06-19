using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Libros;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Libros.Controllers
{
    public class MayorBase:ControladorBase
    {
        public MayorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto,
            ILogger logger):base(options,contexto,logger)
        {
            
        }

        public List<BSumaSaldoRegDto> BalanceSS
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("BalanceSS") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<BSumaSaldoRegDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("BalanceSS", json);
            }
        }

        public List<LMayorRegListaDto> LibroMayor
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("LibroMayor") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<LMayorRegListaDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("LibroMayor", json);
            }
        }

        public List<AsientoDetalleLDDto> LibroDiario
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("LibroDiario") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<AsientoDetalleLDDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("LibroDiario", json);
            }
        }

    }
}
