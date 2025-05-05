namespace gc.infraestructura.EntidadesComunes.Options
{

    /// <summary>
    /// Esta clase se encarga de manejar los modulos y reportes que se pueden imprimir
    /// pero solo a nivel de configuración.
    /// </summary>
    public class DocsManager
    {
        public DocsManager()
        {
            Modulos = [];
        }
        public List<AppModulo> Modulos { get; set; }
        public string ApiReporte { get; set; } = string.Empty;
        //
    }

    public class AppModulo
    {
        public string Id { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public List<Reporte> Reportes { get; set; }= new List<Reporte>();
        public bool Print { get; set; }
        public bool Export { get; set; }
        public bool Email { get; set; }
        public bool Whatsapp { get; set; }
    }

    public class Reporte
    {
        public int Id { get; set; }
        public string[] Titulos { get; set; } = [];
        public string NombreArchivo01 { get; set; } = string.Empty;
        public string NombreArchivo02 { get; set; } = string.Empty;
        public bool ImprimeDuplicado { get; set; }
        public bool ImprimeSoloDuplicado { get; set; }
        public bool ConsultaRealizada { get; set; }
        public List<ArchivosB64> Archivos { get; set; } = new List<ArchivosB64>();     
    }

    public class ArchivosB64
    {
        public string Nombre { get; set; } = string.Empty;
        public string Base64 { get; set; } = string.Empty;
        public bool HayArchivo { get { return !string.IsNullOrEmpty(Base64); } }
    }
}
