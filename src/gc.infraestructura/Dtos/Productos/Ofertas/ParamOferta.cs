

namespace gc.infraestructura.Dtos.Productos.Ofertas
{
    public class ParamOferta
    {
        public decimal Precio { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int TopeVta { get; set; }
        public string OftId { get; set; } = string.Empty;
    }

    public class TipoOfertaDto
    {
        public string oft_id { get; set; } = string.Empty;
        public string oft_desc { get; set; } = string.Empty;
        public string oft_lista { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para recibir datos de confirmación de oferta desde el cliente
    /// </summary>
    public class ConfirmacionOfertaRequestDto
    {
        public List<CanalSeleccionadoDto> Canales { get; set; } = new();
        public CanalSeleccionadoDto? CanalIndividual { get; set; }
        public string ModoSeleccion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public DateTime FechaDesde { get; set; } 
        public DateTime FechaHasta { get; set; } 
        public int TopeVenta { get; set; }
        public string TipoOfertaId { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para canales seleccionados
    /// </summary>
    public class CanalSeleccionadoDto
    {
        public string AdmId { get; set; } = string.Empty;
        public string LpId { get; set; } = string.Empty;
        public string Canal { get; set; } = string.Empty;
        public string AdmNombre { get; set; } = string.Empty;
        public string LpDesc { get; set; } = string.Empty;
    }
}
