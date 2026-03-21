using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos.DocManager
{
    public interface IDocMgServicio
    {
        Task<RespuestaReportDto> ObtenerRepoParaUsuario(string parametros);

    }
}
