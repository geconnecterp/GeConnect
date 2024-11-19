namespace gc.api.core.Entidades
{
    public class CambioClaveDto : EntidadBase
    {
        public string UserName { get; set; }=string.Empty;
        public string? PassAct { get; set; }
        public string? PassNew { get; set; }
        public string? PassNewVer { get; set; }
    }
}
