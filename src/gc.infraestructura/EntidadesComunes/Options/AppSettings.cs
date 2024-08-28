using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Core.EntidadesComunes.Options
{
    public class AppSettings
    {
        public AppSettings()
        {
            Nombre=Sigla=string.Empty;
        }
        /// <summary>
        /// Nombre del sitio o aplicación
        /// </summary>
        public string Nombre{  get; set; }
        /// <summary>
        /// Sigla o Nombre resumido del sitio o aplicación
        /// </summary>
        public string Sigla { get; set; }
        public int IntentosAcceso { get; set; }
        public bool ExceptionManagerEnabled { get; set; }
        public string? RutaBase { get; set; }
        //public string? Token { get; set; }
        public int LimiteAvalancha { get; set; }
        public int NroRegistrosPagina { get; set; }
        public string? RutaTemporal { get; set; }
        public string? RutaLogo { get; set; }
        public bool LogExtension { get; set; }
        public string? RutaRepositorioPDF { get; set; }
        public int FechaVtoCota{ get; set; }
    }
}
