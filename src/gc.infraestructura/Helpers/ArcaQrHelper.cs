using IronBarCode;

namespace gc.infraestructura.Helpers
{
    public static class ArcaQrHelper
    {
        public static byte[] GenerarPng(string contenido)
        {
            var qr = QRCodeWriter.CreateQrCode(contenido, 300);
            return qr.ToPngBinaryData();
        }
    }
}