using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.ABMs.Models
{
    public class CuentaABMNotaModel
    {
        public GridCore<CuentaNotaDto> CuentaNotas { get; set; }
        public NotaModel Nota { get; set; }

        public CuentaABMNotaModel()
        { 
            Nota = new NotaModel();
        }
    }
}
