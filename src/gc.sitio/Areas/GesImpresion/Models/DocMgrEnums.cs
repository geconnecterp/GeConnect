namespace gc.sitio.Areas.GesImpresion.Models
{
    public enum DocumentType
    {
        Original,
        Duplicate,
        Certificate,
        Receipt,
        Report,
        Statement
    }

    public enum ExportType
    {
        PDF,
        Excel,
        TXT
    }

    public enum MessagePriority
    {
        Low,
        Normal,
        High
    }
}
