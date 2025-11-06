using System.Data;

namespace gc.infraestructura.Dtos.ABM
{
    public class AbmGenDto
    {
        public string Objeto { get; set; }=string.Empty;
        public char Abm { get; set; }
        public string Json { get; set; } = string.Empty;
        public string Usuario { get; set; }=string.Empty ;
        public string Administracion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Esta clase tendrá como misión poder enviar más json en el envio para procesos paralelos
    /// </summary>
    public class AbmPlusGenDto:AbmGenDto
    {
        public string Json2 { get; set; } = string.Empty;
        public string Json3 { get; set; } = string.Empty;
        public string Json4 { get; set; } = string.Empty;
        public string Json5 { get; set; } = string.Empty;

        public Guid IdFile { get; set; } 
        public char SoloPLista { get; set; }
        public bool Nuevos { get; set; }
        public bool DatosLogisticos { get; set; }
        public bool Inactivos { get; set; }
        public bool vaciarTemporal { get; set; }       

    }
}
