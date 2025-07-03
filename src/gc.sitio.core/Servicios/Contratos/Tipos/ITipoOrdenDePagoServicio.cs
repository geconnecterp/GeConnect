using gc.infraestructura.Dtos.Tipos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoOrdenDePagoServicio : IServicio<TipoOrdenDePagoDto>
	{
		List<TipoOrdenDePagoDto> ObtenerTiposDeOrdenDePago(string token);
	}
}
