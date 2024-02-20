using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Core.EntidadesComunes.Options
{
    public class AppSettings
    {
        public int IntentosAcceso { get; set; }
        public bool ExceptionManagerEnabled { get; set; }
        public string? RutaBase { get; set; }
        public string? Token { get; set; }
        public int LimiteAvalancha { get; set; }
        public string? RutaTemporal { get; set; }
        public string? RutaLogo { get; set; }
        public bool LogExtension { get; set; }
        public string? RutaRepositorioPDF { get; set; }
    }
}
