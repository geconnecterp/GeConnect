using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class ImpositivoController : ControladorOfertaBase
    {
        // variables para manerjar modulo de impresión
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.IMPOSITIVO.ToString();
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _configuracion;

        private readonly IProducto2Servicio _prod2Sv;
        private readonly IPrecioListaServicio _plSv;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IRubroServicio _rubroServicio;

        public ImpositivoController(IOptions<AppSettings> options, IHttpContextAccessor contexo,
         ILogger<PrecioListaController> logger, IOptions<DocsManager> docsManager,
         IDocManagerServicio docManagerServicio, IPrecioListaServicio servicio,
         ICuentaServicio cuentaServicio,
          IRubroServicio rubroServicio,
          IProducto2Servicio producto2) : base(options, contexo, logger)
        {
            _configuracion = options.Value;

            // inicializo las variables para manejar el modulo de impresión
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManagerServicio;
            _plSv = servicio;
            _prod2Sv = producto2;
            _cuentaServicio = cuentaServicio;
            _rubroServicio = rubroServicio;
        }

        public IActionResult Index()
        {
            string msg = "Error de negocios al cargar la vista de PRESUPUESTOS";
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                string titulo = "Lista de Precios";
                ViewData["Titulo"] = titulo;
                #region Gestor Impresion - Inicializacion de variables

                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
                ViewBag.ImpresionId = _modulo.Reportes[0].Id; //siempre el primer reporte

                _logger?.LogInformation($"Generando Arbol de Archivos del módulo. {MethodBase.GetCurrentMethod()?.Name}");

                //en este mismo acto se cargan los posibles documentos
                //que se pueden imprimir, exportar, enviar por email o whatsapp
                ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion
                InicializaVista();
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, msg);
                TempData["error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msg);
                TempData["error"] = "Hubo un problema al cargar la vista de ETIQUETAS. Si el problema persiste, contacte al administrador.";
            }

            return View();
        }



        private void InicializaVista(bool actualizar = true)
        {
            if (ProveedoresLista.Count == 0 || actualizar)
            {
                ObtenerProveedores(_cuentaServicio, "BI");
            }

            if (RubroLista.Count == 0 || actualizar)
            {
                ObtenerRubros(_rubroServicio);
            }

            var rubs = RubroLista
                .Select(r => new ComboGenDto
                {
                    Id = r.Rub_Id,
                    Descripcion = r.Rub_Id + " - " + r.Rub_Desc
                })
                .ToList();
            ViewBag.Rel02 = HelperMvc<ComboGenDto>.ListaGenerica(rubs);

            var listR01 = new List<ComboGenDto>();
            ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);


            ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);


            ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
            ViewBag.Rel03 = HelperMvc<ComboGenDto>.ListaGenerica(listR01);


            //datos impositivos
            ViewBag.CondicionIva = ComboIVASituacion(_prod2Sv);
            ViewBag.AlicuotaIva = ComboIVAAlicuota(_prod2Sv);
        }
    }
}
