namespace gc.api.core.Entidades
{
    public partial class BilleteraConfiguracion : EntidadBase
    {
        public BilleteraConfiguracion()
        {
            Bc_Id = string.Empty;
            Bc_Ruta_Publickey = string.Empty;
            Bc_Ruta_Privatekey = string.Empty;
        }

        public string Bc_Id { get; set; }
        public string Bc_Ruta_Publickey { get; set; }
        public string Bc_Ruta_Privatekey { get; set; }

    }
}
