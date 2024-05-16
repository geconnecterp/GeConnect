namespace gc.notificacion.api.Modelo
{
    public class OrdenBase
    {
        public string Orden_Id { get; set; }
        public string Orden_Id_Ext { get; set; }
    }
    public class OrdenNotificado : OrdenBase
    {
        public char Orden_Notificada_Ok { get; set; }
    }

    public class OrdenRegistro : OrdenBase
    {
        public char Orden_Solicitada_Ok { get; set; }
    }
}
