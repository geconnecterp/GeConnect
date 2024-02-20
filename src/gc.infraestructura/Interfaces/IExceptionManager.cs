using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Core.Interfaces
{
    public interface IExceptionManager
    {
        Exception HandleException(Exception ex);
    }
}
