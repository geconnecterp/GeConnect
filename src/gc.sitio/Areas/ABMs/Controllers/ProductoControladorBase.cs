using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers
{
    public class ProductoControladorBase : ControladorBase
    {
        public ProductoControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor) :base(options,accessor)
        {
            
        }


        #region ABM
        /// <summary>
        /// Permite verificar que pagina se esta observando.
        /// </summary>
        public int PaginaProd
        {
            get
            {
                var txt = _context.HttpContext.Session.GetString("PaginaProd");
                if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
                {
                    return 0;
                }
                return txt.ToInt();
            }
            set
            {
                var valor = value.ToString();
                _context.HttpContext.Session.SetString("PaginaProd", valor);
            }
        }

        public string DirSortProd
        {
            get
            {
                var txt = _context.HttpContext.Session.GetString("DirSortProd");
                if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
                {
                    return "asc";
                }
                return txt;
            }
            set
            {
                var valor = value.ToString();
                _context.HttpContext.Session.SetString("DirSortProd", valor);
            }
        }

        public List<ABMProductoSearchDto> ProductosBuscados
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("ProductosBuscados");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<ABMProductoSearchDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("ProductosBuscados", json);
            }
        }

        public MetadataGrid MetadataProd
        {
            get
            {
                var txt = _context.HttpContext.Session.GetString("MetadataProd");
                if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
                {
                    return new MetadataGrid();
                }
                return JsonConvert.DeserializeObject<MetadataGrid>(txt); ;
            }
            set
            {
                var valor = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("MetadataProd", valor);
            }

        }

        #endregion
    }
}
