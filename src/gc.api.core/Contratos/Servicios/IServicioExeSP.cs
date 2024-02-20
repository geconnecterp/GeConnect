namespace gc.api.core.Interfaces.Servicios
{
    using gc.api.core.Entidades;
    using System.Collections.Generic;

    public interface IServicioExeSP<T> where T : EntidadBase
    {
        List<T> EjecutarSP(string?sp, params object[] parametros);
    }
}
