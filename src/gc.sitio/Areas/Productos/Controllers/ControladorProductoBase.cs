using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Productos;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Productos.Controllers
{
    public class ControladorProductoBase:ControladorBase
    {
        public ControladorProductoBase(IOptions<AppSettings> options, IHttpContextAccessor contexto,
            ILogger logger) : base(options, contexto, logger)
        {

        }

        public List<ProductoDetalleDto> ProductosDetalle
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("ProductosDetalle") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<ProductoDetalleDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("ProductosDetalle", json);
            }
        }

        public List<ProductoDetalleDto> ProductosDetalleLista
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("ProductosDetalleLista") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<ProductoDetalleDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("ProductosDetalleLista", json);
            }
        }

        public List<ProductoDetalleDto> ProductosDetalleTEMPORAL
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("ProductosDetalleTEMPORAL") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<ProductoDetalleDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("ProductosDetalleTEMPORAL", json);
            }
        }

        public List<ProductoDetalleDto> ProductosDetalleListaTEMPORAL
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("ProductosDetalleListaTEMPORAL") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<ProductoDetalleDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("ProductosDetalleListaTEMPORAL", json);
            }
        }
    }
}
