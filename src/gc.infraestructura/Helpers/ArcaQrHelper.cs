using QRCoder;

namespace gc.infraestructura.Helpers
{
    public static class ArcaQrHelper
    {
        public static byte[] GenerarPng(string contenido)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(contenido, QRCodeGenerator.ECCLevel.Q);
            using var qr = new PngByteQRCode(data);
            return qr.GetGraphic(20);
        }
    }
}