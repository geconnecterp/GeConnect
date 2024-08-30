namespace gc.infraestructura.Dtos.Almacen.Rpr
{
    public class RprResponseDto
    {
        public short Resultado { get; set; }
        public string Resultado_msj { get; set; } = string.Empty;
        public string Box_id_sugerido { get; set; } = string.Empty;

    }
}
