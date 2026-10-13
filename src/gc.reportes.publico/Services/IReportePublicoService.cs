using Geco.Reportes.Publico.Models;

namespace Geco.Reportes.Publico.Services;

public interface IReportePublicoService
{
    Task<DocumentoPdf> DescargarAsync(string codigo, ContextoDescarga contexto, CancellationToken cancellationToken);
}
