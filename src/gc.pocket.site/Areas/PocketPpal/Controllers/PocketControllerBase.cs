using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.OrdenReparto;
using gc.pocket.site.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    public class PocketControllerBase: ControladorBase
    {
        public PocketControllerBase(IOptions<AppSettings> options,
            IHttpContextAccessor httpContext,
            ILogger logger):base(options,httpContext,logger)
        {
            
        }

        /// <summary>
        /// esta variable contiene la OR seleccionada y ACTUAL
        /// </summary>
        public string ORComprobanteActual
        {
            get { return _context.HttpContext?.Session.GetString("ORComprobanteActual") ?? string.Empty; }

            set { HttpContext.Session.SetString("ORComprobanteActual", value); }
        }

        // ✅ NUEVO: Propiedad para almacenar el BOX seleccionado
        public string ORBoxSeleccionado
        {
            get { return _context.HttpContext?.Session.GetString("ORBoxSeleccionado") ?? string.Empty; }

            set { HttpContext.Session.SetString("ORBoxSeleccionado", value); }
        }

        // ✅ NUEVO: Propiedad para almacenar el RUBRO seleccionado
        public string ORRubroSeleccionado
        {
            get { return _context.HttpContext?.Session.GetString("ORRubroSeleccionado") ?? string.Empty; }

            set { HttpContext.Session.SetString("ORRubroSeleccionado", value); }
        }

        // ✅ NUEVO: Propiedad para almacenar la lista de productos de OR en sesión
        public List<ORProductoDto> ORListaProductosActual
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("ORListaProductosActual");
                if (string.IsNullOrEmpty(json))
                {
                    return new List<ORProductoDto>();
                }

                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<ORProductoDto>>(json) 
                           ?? new List<ORProductoDto>();
                }
                catch
                {
                    return new List<ORProductoDto>();
                }
            }

            set
            {
                var json = System.Text.Json.JsonSerializer.Serialize(value);
                HttpContext.Session.SetString("ORListaProductosActual", json);
            }
        }
    }
}
