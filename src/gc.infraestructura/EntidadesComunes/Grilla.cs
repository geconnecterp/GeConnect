namespace gc.infraestructura.Core.EntidadesComunes
{
    public class Grilla
    {
        public Grilla()
        {
            ListaDatos = new object();
        }
        public object ListaDatos { get; set; }
        public int CantidadReg { get; set; }

        public int PrimerRegistro { get; set; }
        public int UltimoRegistro { get; set; }
        public int RegistroFinal { get; set; }
        public int CantidadPaginas { get; set; }
        public int PaginaActual { get; set; }
        public string? Sort { get; set; }
    }
}
