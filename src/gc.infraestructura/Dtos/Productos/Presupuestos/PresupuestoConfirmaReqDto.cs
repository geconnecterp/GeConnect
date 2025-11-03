namespace gc.infraestructura.Dtos.Productos.Presupuestos
{
    public class PresupuestoConfirmaReqDto
    {
        public char Abm { get;set; } // A: alta, B: baja, M: modificacion
        public PresupuestoDto Datos { get; set; } = new();
        public List<PresupuestoProductoDto> Productos { get; set; } = [];
    }
}
