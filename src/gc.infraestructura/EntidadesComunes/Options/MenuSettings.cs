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
        public string Nombre { get; set; }
        public string Sigla { get; set; }
        public string Color { get; set; }
        public string Url { get; set; }
        public bool ConContraste { get; set; }
    }
}
