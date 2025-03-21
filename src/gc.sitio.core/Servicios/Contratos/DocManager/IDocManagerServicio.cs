using gc.infraestructura.Dtos.DocManager;
using iTextSharp.text;

namespace gc.sitio.core.Servicios.Contratos.DocManager
{
    public interface IDocManagerServicio
    {
        void GenerarArchivoPDF<T>(PrintRequestDto<T> request, out MemoryStream ms);

    }
}
