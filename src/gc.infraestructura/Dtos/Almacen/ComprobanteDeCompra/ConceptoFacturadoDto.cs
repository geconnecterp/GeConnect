using System.Runtime.Serialization;

namespace gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra
{
    public class ConceptoFacturadoDto : Dto
    {
		[DataMember]
		public int id { get; set; } = 0; //Valor que se usa para poder identificar un elemento de la colección
		[DataMember]
		public string concepto { get; set; } = string.Empty;
		[DataMember]
		public int cantidad { get; set; } = 1;
		[DataMember]
		public string iva_situacion { get; set; } = string.Empty;
		[DataMember]
		public decimal iva_alicuota { get; set; } = 0.00M;
		[DataMember]
		public decimal subtotal { get; set; } = 0.00M;
		[DataMember]
		public decimal iva { get; set; } = 0.00M;
		[DataMember]
		public decimal total { get; set; } = 0.00M;

		public string getStringFormat(decimal val)
		{
			var _ret = "";
			var aux1 = val.ToString("0.00").Split('.');
			if (aux1.Length == 1)
				return aux1 + ",00";
			else
			{
				if (aux1[1].Length == 1)
					return aux1 + "0";
			}
			return val.ToString("0.00");
		}
	}
}
