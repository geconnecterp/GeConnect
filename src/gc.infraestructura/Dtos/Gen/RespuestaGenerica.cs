namespace gc.infraestructura.Dtos.Gen
{
    public class RespuestaGenerica<T>
    {
        public T? Entidad { get; set; }
        public List<T>? ListaEntidad { get; set; }
        //public GridCoreSmart<T>? GrillaDatos { get; set; }
        public bool Ok { get; set; } = true;
        public bool EsError { get; set; }=false;
        public bool EsWarn { get; set; } = false;
        public string? Mensaje { get; set; }
    }
}
