namespace gc.infraestructura.Core.EntidadesComunes.Options
{
    public class PasswordOptions
    {
        //las mismas propiedades que la configuración
        public int SaltSize { get; set; }
        public int KeySize { get; set; }
        public int Iterations { get; set; }
    }
}
