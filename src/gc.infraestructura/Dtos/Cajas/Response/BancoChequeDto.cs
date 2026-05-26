namespace gc.infraestructura.Dtos.Cajas.Response
{
    /// <summary>
    /// DTO de respuesta con información de banco para cheques
    /// Desde SP: SPGECO_ABM_BCO_CH_Lista
    /// </summary>
    public class BancoChequeDto
    {
        /// <summary>
        /// ID del banco
        /// </summary>
        public string BancoId { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del banco
        /// </summary>
        public string BancoDesc { get; set; } = string.Empty;

        /// <summary>
        /// Plaza del banco (opcional)
        /// </summary>
        public string? Plaza { get; set; }

        /// <summary>
        /// Código interno del banco
        /// </summary>
        public string? CodigoBanco { get; set; }

        /// <summary>
        /// Estado activo/inactivo
        /// </summary>
        public bool Activo { get; set; }
    }
}