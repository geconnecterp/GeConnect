using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Core.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }
}
