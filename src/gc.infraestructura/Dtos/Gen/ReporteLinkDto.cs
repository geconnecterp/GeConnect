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
    }

    public class ReporteLinkResponseDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime ExpiraEnUtc { get; set; }
    }
}
