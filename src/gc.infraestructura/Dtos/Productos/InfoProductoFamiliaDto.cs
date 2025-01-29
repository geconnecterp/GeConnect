
namespace gc.infraestructura.Dtos.Productos
{
	public class InfoProductoFamiliaDto : Dto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
		public string P_Id { get; set; } = string.Empty;
		public string P_Desc { get; set; } = string.Empty;
		public string Cta_Id { get; set; } = string.Empty;
		public string Cta_Denominacion { get; set; } = string.Empty;
		public string Cta_Lista { get; set; } = string.Empty;
		public string Rub_Id { get; set; } = string.Empty;
		public string Rub_Desc { get; set; } = string.Empty;
		public string Rub_Lista { get; set; } = string.Empty;
		public char P_Activo { get; set; }
		public string P_Activo_Desc { get; set; } = string.Empty;
		public string Pg_Id { get; set; } = string.Empty;
		public string Pg_Desc { get; set; } = string.Empty;
		public string Pg_Lista { get; set; } = string.Empty;
	}
}
