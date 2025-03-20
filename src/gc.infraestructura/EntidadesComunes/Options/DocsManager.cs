using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.EntidadesComunes.Options
{
    public class DocsManager
    {
        public DocsManager()
        {
            Modulos = [];
        }

        public List<ModuloDocMngr> Modulos { get; set; }
    }

    public class ModuloDocMngr
    {
        public string Titulo { get; set; } = string.Empty;
        public string Modulo { get; set; }= string.Empty;
        public bool Print { get; set; }
        public bool Export { get; set; }
        public bool Email { get; set; }
        public bool Whatsapp { get; set; }
        public bool ImprimeDuplicado { get; set; }
        public bool ImprimeSoloDuplicado { get; set; }
        public List<float> Anchos { get; set; } = new List<float>();
        public List<string> Titulos { get; set; } = new List<string>();
        public List<string> Columnas { get; set; } = new List<string>();
        public List<string> Titulos2 { get; set; } = new List<string>();
        public List<string> Columnas2 { get; set; } = new List<string>();
    }
}
