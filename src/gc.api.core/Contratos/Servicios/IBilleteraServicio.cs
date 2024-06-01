using gc.api.core.Entidades;

namespace gc.api.core.Contratos.Servicios
{
    public interface IBilleteraServicio : IServicio<Billetera>
    {
        (Billetera, string) FindBilletera(string id);
    }
}
