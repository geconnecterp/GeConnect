using gc.infraestructura.Enumeraciones;

namespace gc.infraestructura.ViewModels
{
    public class DocumentManagerViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public bool ShowExportOption { get; set; }
        public bool ShowPrintOption { get; set; }
        public bool ShowEmailOption { get; set; }
        public bool ShowWhatsAppOption { get; set; }
    }





    public class DocumentViewModel
    {
        public string Id { get; set; }=string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public DocumentType Type { get; set; }
    }

    public class PrintDocumentViewModel
    {
        public List<DocumentViewModel> Documents { get; set; } = new List<DocumentViewModel>();
        //public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();

        //permitiria imprimir duplicados si estuvieran seteados en la configuracion
        public bool ShowDuplicates { get; set; }
        //public bool ShowCertificates { get; set; }
    }

    public class ExportDocumentViewModel
    {
        public ExportType ExportType { get; set; }
        public string FileName { get; set; } = string.Empty;
        public object? Data { get; set; }
        public List<string> AvailableExportTypes { get; set; } = new List<string> { "PDF", "Excel", "TXT" };
    }

    public class EmailDocumentViewModel
    {
        public List<string> ToEmails { get; set; } = new List<string>();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<AttachmentViewModel> Attachments { get; set; } = new List<AttachmentViewModel>();
        public string DefaultTemplate { get; set; } = string.Empty;
    }

    public class WhatsAppDocumentViewModel
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<AttachmentViewModel> Attachments { get; set; } = new List<AttachmentViewModel>();
        public string DefaultTemplate { get; set; } = string.Empty;
    }

    public class AttachmentViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = [];
        public string ContentType { get; set; } = string.Empty;
    }

    public class CommercialAccountViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class ReportHeaderViewModel
    {
        public string CompanyLogo { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyTaxId { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string BranchId { get; set; } = string.Empty;
        public string MainTitle { get; set; } = string.Empty;
        public string SecondaryTitle { get; set; } = string.Empty;
        public string TertiaryTitle { get; set; } = string.Empty;   
    }
}
