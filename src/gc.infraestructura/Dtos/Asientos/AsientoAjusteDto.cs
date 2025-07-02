namespace gc.infraestructura.Dtos.Asientos
{
    public class AsientoAjusteDto
    {
        public int Eje_nro { get; set; }
        public string Ccb_id { get; set; }=string.Empty;
        public string Ccb_desc { get; set; }=string.Empty;
        public decimal Saldo { get; set; }
        public decimal Ajuste { get; set; }
        public decimal Saldo_ajustado { get; set; }
        public bool Ajusta { get; set; }
    }

    public class AsientoAjusteCcbDto
    {
        public int Eje_nro { get; set; }
        public string Ccb_id { get; set; } = string.Empty;
        public string Ccb_desc { get; set; } = string.Empty;
        public int Periodo { get; set; }
        public int Mes { get; set; }
        public decimal Indice { get; set; }
        public decimal Coeficiente { get; set; }
        public decimal Saldo { get; set; }
        public decimal Ajuste { get; set; }
        public decimal Saldo_ajustado { get; set; }
    }

    public class GenerarAsientoAjusteViewModel
    {
        public int Ejercicio { get; set; }
        public DateTime FechaAsiento { get; set; }
        public string CuentaAjuste { get; set; }=string.Empty;
        public List<string> CuentasSeleccionadas { get; set; } = [];
    }

    public class GenerarAsientoAjusteRequestDto
    {
        public int Eje_nro { get; set; }
        public DateTime Fecha_asiento { get; set; }
        public string Cuenta_ajuste { get; set; } = string.Empty;
        public List<string> Cuentas_seleccionadas { get; set; } = [];
    }

    public class GenerarAsientoAjusteResponseDto
    {
        public bool Error { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int AsientoId { get; set; }
    }
}
