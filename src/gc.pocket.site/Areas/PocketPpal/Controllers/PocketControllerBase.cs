using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.OrdenReparto;
using gc.pocket.site.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    public class PocketControllerBase : ControladorBase
    {
        public PocketControllerBase(IOptions<AppSettings> options,
            IHttpContextAccessor httpContext,
            ILogger logger) : base(options, httpContext, logger)
        {
        }

        #region Variables de Sesión OR

        /// <summary>
        /// ✅ Variable ÚNICA de sesión que contiene TODOS los datos de OR
        /// </summary>
        public ORSessionDto ORSession
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("ORSession");
                if (string.IsNullOrEmpty(json))
                {
                    return new ORSessionDto();
                }

                try
                {
                    return JsonSerializer.Deserialize<ORSessionDto>(json)
                           ?? new ORSessionDto();
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "⚠️ Error al deserializar ORSession");
                    return new ORSessionDto();
                }
            }

            set
            {
                try
                {
                    var json = JsonSerializer.Serialize(value);
                    HttpContext.Session.SetString("ORSession", json);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "❌ Error al serializar ORSession");
                    throw;
                }
            }
        }

        #endregion


        #region Métodos Auxiliares OR



        /// <summary>
        /// Limpia TODAS las variables de sesión relacionadas con OR
        /// </summary>
        public void LimpiarSesionOR()
        {
            HttpContext.Session.Remove("ORSession");
            _logger?.LogInformation("🧹 Sesión OR limpiada completamente");
        }

        /// <summary>
        /// Verifica si existe una OR en proceso válida
        /// </summary>
        public bool TieneOREnProceso()
        {
            var session = ORSession;
            return !string.IsNullOrEmpty(session?.ORComprobanteActual);
        }

        /// <summary>
        /// Obtiene el producto actual según el ID seleccionado
        /// </summary>
        public ORProductoDto? ObtenerProductoActualOR()
        {
            var session = ORSession;
            if (string.IsNullOrEmpty(session?.ORProductoSeleccionado))
                return null;

            return session.ORListaProductosActual?
                .FirstOrDefault(x => x.p_id == session.ORProductoSeleccionado);
        }

        #endregion
    }
}
