using gc.infraestructura.Dtos.Asientos;

namespace gc.api.core.Contratos.Servicios.Libros
{
    public interface IApiLDiarioServicio
    {
        List<AsientoDetalleLDDto> ObtenerAsientoLibroDiario(
            int eje_nro,
            bool periodo,
            DateTime desde,
            DateTime hasta,
            bool hasCarga,
            DateTime Cdesde,
            DateTime Chasta,
            string movimientos,
            bool conTemporales,
            int regs,
            int pag,
            string orden);

        List<LibroDiarioResumen> ObtenerAsientoLibroDiarioResumen(
           int eje_nro,
           bool periodo,
           DateTime desde,
           DateTime hasta,
           bool hasCarga,
           DateTime Cdesde,
           DateTime Chasta,
           string movimientos,
           bool conTemporales,
           int regs,
           int pag,
           string orden);
    }
}
