namespace gc.infraestructura.EntidadesComunes.Options
{
	public class ImportacionExtracto
	{
		public ImportacionExtracto()
		{
			Formatos = [];
		}
		public List<FormatoExtractoConfig> Formatos { get; set; } = new();
	}
	public class CampoFormato
	{
		public string Nombre { get; set; } = "";
		public int? Inicio { get; set; }
		public int? Longitud { get; set; }
		public int? Posicion { get; set; }
		public string? Formato { get; set; }
	}

	public class FormatoExtractoConfig
	{
		public string Id { get; set; } = "";
		public string Nombre { get; set; } = "";
		public string Tipo { get; set; } = ""; // "Fijo", "Delimitado", "XLSX"
		public string? Separador { get; set; }
		public int? LongitudEsperada { get; set; }
		public string? Hoja { get; set; }
		public List<CampoFormato> Campos { get; set; } = new();
		public List<string> Columnas { get; set; } = new(); // solo para XLSX
	}
}
