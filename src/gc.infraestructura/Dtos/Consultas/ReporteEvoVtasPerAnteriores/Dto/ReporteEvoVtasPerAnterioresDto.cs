
namespace gc.infraestructura.Dtos
{
	public class ReporteEvoVtasPerAnterioresDto : Dto
	{
		public string p_id { get; set; } = string.Empty;
		public string p_id_barrado { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public string rubg_id { get; set; } = string.Empty;
		public string rubg_desc { get; set; } = string.Empty;
		public string sec_id { get; set; } = string.Empty;
		public string sec_desc { get; set; } = string.Empty;

		public int periodo1 { get; set; }
		public int periodo2 { get; set; }
		public int periodo3 { get; set; }
		public int periodo4 { get; set; }

		public decimal vtas_cantidad1 { get; set; }
		public decimal vtas_cantidad2 { get; set; }
		public decimal vtas_cantidad3 { get; set; }
		public decimal vtas_cantidad4 { get; set; }

		public decimal vtas_facturacion1 { get; set; }
		public decimal vtas_facturacion2 { get; set; }
		public decimal vtas_facturacion3 { get; set; }
		public decimal vtas_facturacion4 { get; set; }

		// -----------------------------
		// DIFERENCIAS DE CANTIDADES
		// -----------------------------
		public decimal DifCant_1_2 => vtas_cantidad1 - vtas_cantidad2;
		public decimal DifCant_2_3 => vtas_cantidad2 - vtas_cantidad3;
		public decimal DifCant_3_4 => vtas_cantidad3 - vtas_cantidad4;

		// -----------------------------
		// % DIFERENCIAS DE CANTIDADES
		// -----------------------------
		public decimal PorcCant_1_2 => CalcularPorcentaje(vtas_cantidad1, vtas_cantidad2);
		public decimal PorcCant_2_3 => CalcularPorcentaje(vtas_cantidad2, vtas_cantidad3);
		public decimal PorcCant_3_4 => CalcularPorcentaje(vtas_cantidad3, vtas_cantidad4);

		// -----------------------------
		// DIFERENCIAS DE FACTURACIÓN
		// -----------------------------
		public decimal DifFact_1_2 => vtas_facturacion1 - vtas_facturacion2;
		public decimal DifFact_2_3 => vtas_facturacion2 - vtas_facturacion3;
		public decimal DifFact_3_4 => vtas_facturacion3 - vtas_facturacion4;

		// -----------------------------
		// % DIFERENCIAS DE FACTURACIÓN
		// -----------------------------
		public decimal PorcFact_1_2 => CalcularPorcentaje(vtas_facturacion1, vtas_facturacion2);
		public decimal PorcFact_2_3 => CalcularPorcentaje(vtas_facturacion2, vtas_facturacion3);
		public decimal PorcFact_3_4 => CalcularPorcentaje(vtas_facturacion3, vtas_facturacion4);

		// -----------------------------
		// FUNCIÓN INTERNA DE PORCENTAJE
		// -----------------------------
		private decimal CalcularPorcentaje(decimal actual, decimal anterior)
		{
			if (anterior == 0)
			{
				if (actual == 0) return 0;
				return 100; // criterio: si antes era 0 y ahora hay valor → +100%
			}

			return ((actual - anterior) / anterior) * 100;
		}
	}

}
