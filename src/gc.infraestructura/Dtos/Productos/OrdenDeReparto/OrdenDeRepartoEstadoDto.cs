
namespace gc.infraestructura.Dtos.Productos.OrdenDeReparto
{
	public class OrdenDeRepartoEstadoDto : Dto
	{
		public string ore_id { get; set; } = string.Empty;
		public string ore_desc { get; set; } = string.Empty;

		private static readonly Dictionary<string, string> Estados = new()
		{
			{ "C", "Consolidado" },
			{ "E", "Entregado" },
			{ "F", "Facturado" },
			{ "O", "En Curso" },
			{ "T", "A Facturar" }
		};

		//Uso:
		//var desc = OrdenDeRepartoEstadoDto.ObtenerDescripcion("O"); 
		// "En Curso"
		public static string ObtenerDescripcion(string id)
		{
			return Estados.TryGetValue(id.ToUpper(), out var desc)
				? desc
				: "Estado desconocido";
		}

		//Uso:
		//string id = OrdenDeRepartoEstadoDto.ObtenerId(OrdenDeRepartoEstado.EnCurso);
		// id = "O"
		public static string ObtenerId(OrdenDeRepartoEstado estado)
		{
			return estado switch
			{
				OrdenDeRepartoEstado.Consolidado => "C",
				OrdenDeRepartoEstado.Entregado => "E",
				OrdenDeRepartoEstado.Facturado => "F",
				OrdenDeRepartoEstado.EnCurso => "O",
				OrdenDeRepartoEstado.AFacturar => "T",
				_ => string.Empty
			};
		}

		public static OrdenDeRepartoEstado? ObtenerEstado(string id)
		{
			return id.ToUpper() switch
			{
				"C" => OrdenDeRepartoEstado.Consolidado,
				"E" => OrdenDeRepartoEstado.Entregado,
				"F" => OrdenDeRepartoEstado.Facturado,
				"O" => OrdenDeRepartoEstado.EnCurso,
				"T" => OrdenDeRepartoEstado.AFacturar,
				_ => null
			};
		}

	}

	public enum OrdenDeRepartoEstado
	{
		Consolidado = 'C',
		Entregado = 'E',
		Facturado = 'F',
		EnCurso = 'O',
		AFacturar = 'T'
	}

}
