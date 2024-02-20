using System;
using System.Diagnostics;

namespace gc.infraestructura.Core.Interfaces
{
    public interface ILoggerHelper
    {
        Exception Log(Exception ex);
        void Log(TraceEventType tipo, string mensaje);
    }
}
