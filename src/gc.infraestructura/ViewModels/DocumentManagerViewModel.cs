using gc.infraestructura.Enumeraciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace gc.infraestructura.ViewModels
{
    public class DocumentManagerViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Titulo { get; set; } = "";
        public bool ShowPrintOption { get; set; } = true;
        public bool ShowExportOption { get; set; } = true;
        public bool ShowEmailOption { get; set; } = true;
        public bool ShowWhatsAppOption { get; set; } = true;
        public PrintDocumentViewModel PrintModel { get; set; } = new PrintDocumentViewModel();
        public ExportDocumentViewModel ExportModel { get; set; } = new ExportDocumentViewModel();
        public EmailDocumentViewModel EmailModel { get; set; } = new EmailDocumentViewModel();
        public WhatsAppDocumentViewModel WhatsAppModel { get; set; } = new WhatsAppDocumentViewModel();
        //public string SourceApp { get; set; }
        //public string EntityId { get; set; }
        //public string EntityType { get; set; }        

    }

    public class DocumentViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
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
        public string FileName { get; set; }
        public object Data { get; set; }
        public List<string> AvailableExportTypes { get; set; } = new List<string> { "PDF", "Excel", "TXT" };
    }

    public class EmailDocumentViewModel
    {
        public List<string> ToEmails { get; set; } = new List<string>();
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<AttachmentViewModel> Attachments { get; set; } = new List<AttachmentViewModel>();
        public string DefaultTemplate { get; set; }
    }

    public class WhatsAppDocumentViewModel
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public List<AttachmentViewModel> Attachments { get; set; } = new List<AttachmentViewModel>();
        public string DefaultTemplate { get; set; }
    }

    public class AttachmentViewModel
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }

    public class CommercialAccountViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BusinessName { get; set; }
        public string TaxId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }

    public class ReportHeaderViewModel
    {
        public string CompanyLogo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyTaxId { get; set; }
        public string CompanyAddress { get; set; }
        public string BranchId { get; set; }
        public string MainTitle { get; set; }
        public string SecondaryTitle { get; set; }
        public string TertiaryTitle { get; set; }
    }
}
