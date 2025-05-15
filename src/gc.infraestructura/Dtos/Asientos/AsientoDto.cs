using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Asientos
{
    public class AsientoDto
    {
        public string dia_movi { get; set; } = string.Empty;
        public DateTime dia_fecha { get; set; }
        public string dia_tipo { get; set; } = string.Empty;
        public string dia_lista { get; set; } = string.Empty;
        public string dia_desc_asiento { get; set; } = string.Empty;
        public char dia_anulado { get; set; }
        public DateTime dia_fecha_sistema { get; set; }
        public decimal dia_saldo { get; set; }
        public int sin_ccb { get; set; }
        public bool revisable { get; set; }
        public string revisable_desc { get; set; } = string.Empty;

    }

    public class AsientoGridDto : AsientoDto
    {
        public int Total_registros { get; set; }
        public int Total_paginas { get; set; }
    }
}
