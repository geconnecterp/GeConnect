namespace gc.api.core.Entidades
{
    public partial class TipoDocumento : EntidadBase
    {
        public TipoDocumento()
        {
            Descripcion = string.Empty;
            Hasar = string.Empty;
            Epson = string.Empty;
        }

        public int Id { get; set; }
        public string Descripcion { get; set; }
        public string Hasar { get; set; }
        public string Epson { get; set; }



    }
}
