using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Gen
{
    public class ReporteLinkDto
    {
        public long Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string PayloadJson { get; set; } = string.Empty;

        public DateTime FechaCreacionUtc { get; set; }
        public DateTime FechaExpiracionUtc { get; set; }

        public bool Usado { get; set; }
        public DateTime? FechaUsoUtc { get; set; }

        public string? ClienteId { get; set; }
        public string? CreadoPor { get; set; }
        public int Estado { get; set; }
        public long? AccesoId { get; set; }
        public int MaxDescargas { get; set; }
        public int CantidadDescargas { get; set; }
        public DateTime? FechaPrimerIntentoUtc { get; set; }
        public DateTime? FechaUltimaDescargaUtc { get; set; }
        public DateTime? FechaVentanaHastaUtc { get; set; }
    }

    public class ReporteLinkResponseDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime ExpiraEnUtc { get; set; }
        public int MaxDescargas { get; set; }
        public int VentanaDescargaMinutos { get; set; }
    }

    public class ReporteLinkAccesoResponseDto
    {
        public ReporteSolicitudDto Solicitud { get; set; } = new();
        public long AccesoId { get; set; }
        public int MaxDescargas { get; set; }
        public int CantidadDescargas { get; set; }
        public DateTime? FechaVentanaHastaUtc { get; set; }
    }

    public class ReporteLinkAccesoContextoDto
    {
        public string? Ip { get; set; }
        public string? UserAgent { get; set; }
        public string? Referer { get; set; }
    }

    public class ReporteLinkDescargaDto
    {
        public string Codigo { get; set; } = string.Empty;
        public long AccesoId { get; set; }
        public long? Bytes { get; set; }
        public int? DuracionMs { get; set; }
        public int? ResultadoHttp { get; set; }
        public string? Detalle { get; set; }
    }

    public class ReporteLinkOperacionResponseDto
    {
        public int Estado { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int CantidadDescargas { get; set; }
        public int MaxDescargas { get; set; }
    }
}
