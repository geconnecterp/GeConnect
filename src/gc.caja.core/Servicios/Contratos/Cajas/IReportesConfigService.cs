using gc.infraestructura.Dtos.Cajas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    /// <summary>
    /// ✅ NUEVO v10.0: Servicio para gestionar configuración de reportes
    /// Lee la sección "Reportes" del appsettings.json y la expone al sistema
    /// </summary>
    public interface IReportesConfigService
    {
        /// <summary>
        /// Obtiene todos los reportes configurados
        /// </summary>
        List<ReporteConfig> ObtenerTodos();

        /// <summary>
        /// Obtiene un reporte por su Key (Ej: "A", "B")
        /// </summary>
        ReporteConfig? ObtenerPorKey(string key);

        /// <summary>
        /// Obtiene el ID del reporte según Key (Ej: "A" → "67")
        /// </summary>
        string? ObtenerIdPorKey(string key);
    }
}
