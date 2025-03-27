using gc.infraestructura.Dtos.DocManager;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.ViewModels;
using iTextSharp.text;

namespace gc.sitio.core.Servicios.Contratos.DocManager
{
    public interface IDocManagerServicio
    {
        List<MenuRoot> GeneraArbolArchivos(AppModulo modulo);
        void GenerarArchivoPDF<T>(PrintRequestDto<T> request, out MemoryStream ms, List<string> titulos, float[] anchos,bool datosCliente);
        DocumentManagerViewModel InicializaObjeto(string titulo, AppModulo modulo);
        List<MenuRoot> MarcarConsultaRealizada(List<MenuRoot> reportes, AppReportes consulta, int orden,string archB64,string tipoDato);
    }
}
