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
        public string Nombre{  get; set; } = string.Empty;  
        /// <summary>
        /// Sigla o Nombre resumido del sitio o aplicación
        /// </summary>
        public string Sigla { get; set; } = string.Empty;
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
        public string MenuId { get; set; } = string.Empty;
        public string FolderArchivo { get; set; } = string.Empty;

        public string ServerSMTP { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;    
        public string CredUserEmail { get; set; } = string.Empty;
        public string CredPass { get; set; } = string.Empty;
        public bool EnabledSSL { get; set; }
        public string RepoApiUrl { get; set; } = string.Empty;

    }
}
