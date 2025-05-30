using gc.api.core.Contratos.Servicios.ABM;
using gc.api.core.Contratos.Servicios.Asientos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.Asientos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Asientos.Controllers
{
    [Area("Asientos")]
    public class AsientoDetalleController : ControladorBase
    {
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.ASTEMP.ToString();

        private readonly IAsientoFrontServicio _asientoServicio;
        private readonly IAsientoDefinitivoServicio _asDefSv;
        private readonly IAbmServicio _abmSv;
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _appSettings;

        private List<UsuAsientoDto> UsuariosEjercicioLista { get; set; } = [];

        public AsientoDetalleController(
            IOptions<AppSettings> options, IOptions<DocsManager> docsManager,
            IHttpContextAccessor contexto,
            ILogger<AsientoDetalleController> logger,
            IAsientoFrontServicio asientoServicio,
            IAsientoDefinitivoServicio asDefSv,
            IAbmServicio abm,
            IDocManagerServicio docManager
            ) : base(options, contexto, logger)
        {
            _asientoServicio = asientoServicio;
            _asDefSv = asDefSv;
            _appSettings = options.Value;
            _abmSv = abm;
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManager;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
