using gc.infraestructura.Dtos.Cajas.Request;
using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.Cajas
{
    /// <summary>
    /// ✅ DTO para estructura del archivo de backup
    /// Compatible con Newtonsoft.Json
    /// </summary>
    public class BackupDataDto
    {
        [JsonProperty("cajaId")]
        public string CajaId { get; set; } = string.Empty;

        [JsonProperty("usuarioId")]
        public string UsuarioId { get; set; } = string.Empty;

        [JsonProperty("fechaInicio")]
        public DateTime FechaInicio { get; set; }

        [JsonProperty("fechaUltimaActualizacion")]
        public DateTime FechaUltimaActualizacion { get; set; }

        [JsonProperty("cantidadProductos")]
        public int CantidadProductos { get; set; }

        [JsonProperty("productos")]
        public List<ProductoDatosResponseDto> Productos { get; set; } = new();
    }
}
