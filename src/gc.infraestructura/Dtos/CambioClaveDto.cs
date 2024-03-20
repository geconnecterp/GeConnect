using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Dtos
{
    public class CambioClaveDto : Dto
    {
        public string? PassAct { get; set; }
        public string? PassNew { get; set; }
        public string? PassNewVer { get; set; }
    }
}
