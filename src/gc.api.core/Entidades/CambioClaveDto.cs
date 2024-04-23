using gc.api.core.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace gc.api.core.Entidades

{
    public class CambioClaveDto : EntidadBase
    {
        public string? PassAct { get; set; }
        public string? PassNew { get; set; }
        public string? PassNewVer { get; set; }
    }
}
