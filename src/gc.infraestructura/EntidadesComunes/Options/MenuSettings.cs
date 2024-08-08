using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.EntidadesComunes.Options
{
    public class MenuSettings
    {
        public MenuSettings()
        {
            Aplicaciones = new() ;
        }
        public List<AppItem> Aplicaciones { get; set; }
    }

    public class AppItem
    {

        public int Orden { get; set; }
        public string Nombre { get; set; }=string.Empty;
        public string Sigla { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool ConContraste { get; set; }
        public int str { get; set; }
        public List<AppItem> Aplicaciones { get; set; } = new();
    }
}
