using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Core.EntidadesComunes
{
    public class ErrorExceptionValidation
    {
        public ErrorExceptionValidation()
        {
            Error = new ExceptionValidation[] { };
        }
        public ExceptionValidation[] Error { get; set; }
    }
}
