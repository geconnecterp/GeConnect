using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.ViewModels
{
    public class ErroresFluentValidatorVM
    {
        public ErroresFluentValidatorVM()
        {
            Titulo = string.Empty;
            Mensajes = new();
        }

        public string Titulo { get; set; }
        public List<string> Mensajes { get; set; }
    }
}
