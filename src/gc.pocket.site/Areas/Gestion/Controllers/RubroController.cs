using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.pocket.site.Areas.Gestion.Controllers
{
    [Area("Gestion")]
    public class RubroController : ControladorBase
    {
        private readonly ILogger<RubroController> _logger;
        private readonly AppSettings _appSettings;

        public RubroController(ILogger<RubroController> logger, IOptions<AppSettings> options, IHttpContextAccessor context) : base(options, context)
        {
            _appSettings = options.Value;
            _logger = logger;
        }

        [HttpPost]
        [Route("BuscarRubro")]
        public JsonResult Buscar(string prefix)
        {
            //var nombres = await _provSv.BuscarAsync(new QueryFilters { Search = prefix }, TokenCookie);
            //var lista = nombres.Item1.Select(c => new EmpleadoVM { Nombre = c.NombreCompleto, Id = c.Id, Cuil = c.CUIT });
            var rubros = RubroLista.Where(x => x.Rub_Desc.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
            return Json(rubros);
        }
    }
}
