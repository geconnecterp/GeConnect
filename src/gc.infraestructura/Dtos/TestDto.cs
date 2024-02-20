using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos
{
    public class TestDto
    {
        public TestDto()
        {
            DatoStr = "";
        }
        public int Id { get; set; }
        public int DatoInt { get; set; }
        public string DatoStr { get; set; }
        public bool DatoBool { get; set; } = false;
    }
}
