namespace gc.infraestructura.Core.EntidadesComunes
{
    public class ExceptionValidation
    {
        public int Status { get; set; }
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public string? TypeException { get; set; }
    }

    public class ErrorResponse
    {
        public List<ExceptionValidation> Error { get; set; } = new();
    }
}
