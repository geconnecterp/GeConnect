﻿using IronBarCode;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace gc.infraestructura.Core.Helpers
{
    public static class HelperGen
    {
        public static string EncodeStr2B64(string cadena)
        {
            var txtBytes = Encoding.UTF8.GetBytes(cadena);
            return Convert.ToBase64String(txtBytes);
        }

        public static string DecodeB642Str(string b64)
        {
            var b64EncBytes = Convert.FromBase64String(b64);
            return Encoding.UTF8.GetString(b64EncBytes);
        }

        public static byte[] EncodingToBytes(string cadena)
        {
            return Encoding.Default.GetBytes(cadena);
        }

        public static string EncodingToStr(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static string ObtenerCodigoBarra(int grupo, int subgrupo, int correlativoSubgrupo)
        {
            //4 digitos Grupo + 4 digitos Subgrupo + 4 digitos Codigos Correlativo de Subgrupo           
            return $"{grupo.ToString().PadLeft(4, '0')}{subgrupo.ToString().PadLeft(4, '0')}{correlativoSubgrupo.ToString().PadLeft(4, '0')}";
        }
        public static decimal RedondeoEnDecena(decimal valor)
        {
            return Math.Ceiling(valor / 10) * 10;
        }

        public static double RedondeoEnDecena(double valor)
        {
            return Math.Ceiling(valor / 10) * 10;
        }

        /// <summary>
        /// Se valida numero de celulares argentinos
        /// </summary>
        /// <param name="celular"></param>
        /// <returns></returns>
        public static bool ValidarCelular(string celular)
        {
            //will match +61 or +61- or 0 or nothing followed by a nine digit number
            return Regex.Match(celular,@"^[0-9]{10|11}$").Success;
            //to vary this, replace 61 with an international code of your choice 
            //or remove [\+]?61[-]? if international code isn't needed
            //{8} is the number of digits in the actual phone number less one
        }

        public static bool ValidarCorreoElectronico(string correo)
        {
            //string pattern = @"/^(([^<>()\[\]\.,;:\s@”]+(\.[^<>()\[\]\.,;:\s@”]+)*)|(\”.+\”))@(([^<>()[\]\.,;:\s@”]+\.)+[^<>()[\]\.,;:\s@”]{2,})$/";

            //return Regex.IsMatch(pattern, correo,RegexOptions.IgnoreCase);
            try
            {
                var addr = new MailAddress(correo);
                return addr.Address==correo;
            }
            catch 
            {
                return false;
            }
        }

        public static string[] ObtenerMeses(int anio)
        {
            List<string> meses = new List<string>();

            for (int i = 1; i <= 12; i++)
                meses.Add(string.Format("Mes {0}", i + (anio - 1) * 12));

            return meses.ToArray();



        }

        public static string[] Meses ={  "Enero",
                                         "Febrero",
                                         "Marzo",
                                         "Abril",
                                         "Mayo",
                                         "Junio",
                                         "Julio",
                                         "Agosto",
                                         "Septiembre",
                                         "Octubre",
                                         "Noviembre",
                                         "Diciembre"};

        public static string ObtenerFechaEnTexto(DateTime fecha)
        {
            var txt= $"{fecha.Day} de {Meses[fecha.Month - 1]} de {fecha.Year}";
            return txt;
        }


        public static string EnLetras(string num)
        {
            string res, dec = "";
            Int64 entero;
            int decimales;
            double nro;

            try
            {
                nro = Math.Abs(Convert.ToDouble(num));
            }

            catch
            {
                return "";
            }

            entero = Convert.ToInt64(Math.Truncate(nro));
            decimales = Convert.ToInt32(Math.Round((nro - entero) * 100, 2));
            if (decimales > 0)
            {
                dec = " CON " + decimales.ToString() + "/100";
            }
            else
            {
                dec = " 00/100";
            }

            res = ToText(Convert.ToDouble(entero)) + dec;
            return res;
        }

        private static string ToText(double value)
        {
            string Num2Text = "";
            value = Math.Truncate(value);
            if (value == 0) Num2Text = "CERO";
            else if (value == 1) Num2Text = "UNO";
            else if (value == 2) Num2Text = "DOS";
            else if (value == 3) Num2Text = "TRES";
            else if (value == 4) Num2Text = "CUATRO";
            else if (value == 5) Num2Text = "CINCO";
            else if (value == 6) Num2Text = "SEIS";
            else if (value == 7) Num2Text = "SIETE";
            else if (value == 8) Num2Text = "OCHO";
            else if (value == 9) Num2Text = "NUEVE";
            else if (value == 10) Num2Text = "DIEZ";
            else if (value == 11) Num2Text = "ONCE";
            else if (value == 12) Num2Text = "DOCE";
            else if (value == 13) Num2Text = "TRECE";
            else if (value == 14) Num2Text = "CATORCE";
            else if (value == 15) Num2Text = "QUINCE";
            else if (value < 20) Num2Text = "DIECI" + ToText(value - 10);
            else if (value == 20) Num2Text = "VEINTE";
            else if (value < 30) Num2Text = "VEINTI" + ToText(value - 20);
            else if (value == 30) Num2Text = "TREINTA";
            else if (value == 40) Num2Text = "CUARENTA";
            else if (value == 50) Num2Text = "CINCUENTA";
            else if (value == 60) Num2Text = "SESENTA";
            else if (value == 70) Num2Text = "SETENTA";
            else if (value == 80) Num2Text = "OCHENTA";
            else if (value == 90) Num2Text = "NOVENTA";
            else if (value < 100) Num2Text = ToText(Math.Truncate(value / 10) * 10) + " Y " + ToText(value % 10);
            else if (value == 100) Num2Text = "CIEN";
            else if (value < 200) Num2Text = "CIENTO " + ToText(value - 100);
            else if ((value == 200) || (value == 300) || (value == 400) || (value == 600) || (value == 800)) Num2Text = ToText(Math.Truncate(value / 100)) + "CIENTOS";
            else if (value == 500) Num2Text = "QUINIENTOS";
            else if (value == 700) Num2Text = "SETECIENTOS";
            else if (value == 900) Num2Text = "NOVECIENTOS";
            else if (value < 1000) Num2Text = ToText(Math.Truncate(value / 100) * 100) + " " + ToText(value % 100);
            else if (value == 1000) Num2Text = "MIL";
            else if (value < 2000) Num2Text = "MIL " + ToText(value % 1000);
            else if (value < 1000000)
            {
                Num2Text = ToText(Math.Truncate(value / 1000)) + " MIL";
                if ((value % 1000) > 0) Num2Text = Num2Text + " " + ToText(value % 1000);
            }
            else if (value == 1000000) Num2Text = "UN MILLON";
            else if (value < 2000000) Num2Text = "UN MILLON " + ToText(value % 1000000);
            else if (value < 1000000000000)
            {

                Num2Text = ToText(Math.Truncate(value / 1000000)) + " MILLONES ";
                if ((value - Math.Truncate(value / 1000000) * 1000000) > 0) Num2Text = Num2Text + " " + ToText(value - Math.Truncate(value / 1000000) * 1000000);
            }
            else if (value == 1000000000000) Num2Text = "UN BILLON";
            else if (value < 2000000000000) Num2Text = "UN BILLON " + ToText(value - Math.Truncate(value / 1000000000000) * 1000000000000);
            else
            {
                Num2Text = ToText(Math.Truncate(value / 1000000000000)) + " BILLONES";
                if ((value - Math.Truncate(value / 1000000000000) * 1000000000000) > 0) Num2Text = Num2Text + " " + ToText(value - Math.Truncate(value / 1000000000000) * 1000000000000);
            }
            return Num2Text;
        }

        public static string GenerarPeriodoMmAaaa(DateTime fecha)
        {
            return string.Format("{0}/{1}", fecha.Month.ToString().PadLeft(2, '0'), fecha.Year);
        }

        public static string GenerarPeriodoMmAaaa()
        {
            return GenerarPeriodoMmAaaa(DateTime.Today);
        }

        public static string ObtenerHMACtoB64(string text, string key)
        {
            key ??= ""; //si es null se le asigna ""

            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToBase64String(hash);
            }
        }

        public static string ObtenerHMACtoHex(string text, string key, bool useUTF8 = true)
        {
            key ??= ""; //si es null se le asigna ""
            if (useUTF8)
            {
                using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
                {
                    var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                    return Convert.ToHexString(hash);
                }
            }
            else
            {
                using (var hmacsha256 = new HMACSHA256(Encoding.ASCII.GetBytes(key)))
                {
                    var hash = hmacsha256.ComputeHash(Encoding.ASCII.GetBytes(text));
                    return Convert.ToHexString(hash);
                }
            }
        }

        public static string ObtenerHMACtoHexV2(string text, string key)
        {
            key ??= ""; //si es null se le asigna ""

            using (var hmac = new HMACSHA3_256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToHexString(hash);
            }

            //using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            //{
            //    var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            //    return Convert.ToHexString(hash);
            //}

        }

        public static string RSAEncryptToB64(string texto, string rutaPublicKey)
        {
            byte[] textByte = Encoding.UTF8.GetBytes(texto);
            RSA rsa = new RSACryptoServiceProvider(1024);

            Stream fse = File.OpenRead(rutaPublicKey);
            string xmle = new StreamReader(fse).ReadToEnd();
            rsa.FromXmlString(xmle);

            byte[] encrypt = rsa.Encrypt(textByte, RSAEncryptionPadding.Pkcs1);

            return Convert.ToBase64String(encrypt);
        }

        public static string RSADesencrypFromB64(string textB64, string rutaPrivateKey)
        {
            byte[] encrypt = Convert.FromBase64String(textB64);

            RSA des = new RSACryptoServiceProvider(1024);
            Stream fsd = File.OpenRead(Path.Combine(rutaPrivateKey));
            string xmld = new StreamReader(fsd).ReadToEnd();
            des.FromXmlString(xmld);

            byte[] desencr = des.Decrypt(encrypt, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(desencr);
        }

        public static string RSAEncryptToHEX(string texto, string rutaPublicKey)
        {
            byte[] textByte = Encoding.UTF8.GetBytes(texto);
            RSA rsa = new RSACryptoServiceProvider(1024);

            Stream fse = File.OpenRead(rutaPublicKey);
            string xmle = new StreamReader(fse).ReadToEnd();
            rsa.FromXmlString(xmle);

            byte[] encrypt = rsa.Encrypt(textByte, RSAEncryptionPadding.Pkcs1);

            var b64 = Convert.ToBase64String(encrypt);

            //convertir en hex
            return ConvierteStrToHex(b64);
        }

        public static string RSADesencrypFromHEX(string hex, string rutaPrivateKey)
        {
            #region Se convierte la cadena hex en texto.  
            string b64 = ConvierteHexToStr(hex);
            #endregion

            byte[] encrypt = Convert.FromBase64String(b64);

            RSA des = new RSACryptoServiceProvider(1024);
            Stream fsd = File.OpenRead(Path.Combine(rutaPrivateKey));
            string xmld = new StreamReader(fsd).ReadToEnd();
            des.FromXmlString(xmld);

            byte[] desencr = des.Decrypt(encrypt, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(desencr);
        }

        public static string ConvierteStrToB64(this string str)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
        }

        public static string ConvierteB64ToStr(this string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }


        public static string ConvierteStrToHex(string str)
        {
            return BitConverter.ToString(Encoding.ASCII.GetBytes(str)).Replace("-", "");
        }

        public static string ConvierteHexToStr(string hex)
        {
            int length = hex.Length / 2;
            string texto = string.Empty;

            for (int i = 0; i < length; i++)
            {
                texto += Convert.ToChar(Convert.ToInt32(hex.Substring(2 * i, 2), 16));
            }

            return texto;
        }

        /// <summary>
        /// Se codifica un id en code bar 3of9 y se devuelve en una imagen en b64. 
        /// </summary>
        /// <returns></returns>
        public static string GeneraIdEnCodeBar3of9(string id)
        {
            string b64 = string.Empty;
            // Generar el código de barras  
            var barcode = BarcodeWriter.CreateBarcode($"*{id}*", BarcodeEncoding.Code39);

            using (MemoryStream ms = new MemoryStream())
            {
                // Convertir la imagen del código de barras a bytes  
                byte[] barcodeBytes = barcode.ToPngBinaryData(); // Cambiado de ToByteArray a ToPngBinaryData  

                // Cargar la imagen desde los bytes usando ImageSharp  
                using (var img = Image.Load<Rgba32>(barcodeBytes))
                {
                    // Crear una instancia del PngEncoder  
                    var pngEncoder = new PngEncoder();

                    // Guardar la imagen en el MemoryStream usando SixLabors.ImageSharp  
                    img.Save(ms, pngEncoder);
                }

                byte[] data = ms.ToArray();
                b64 = Convert.ToBase64String(data);
            }

            return b64;
        }

        public static string GeneraIdEnCodeBar3of9WithText(string id)
        {
            string b64 = string.Empty;
            //se crea el codigo de barras
            var barcode = BarcodeWriter.CreateBarcode(id, BarcodeEncoding.Code39, 100, 50);
            var font = SixLabors.Fonts.SystemFonts.CreateFont("Arial", 12);

            using (var finalImg = new Image<Rgba32>(barcode.Width, barcode.Height + 50))
            {
                finalImg.Mutate(ctx =>
                {
                    // Dibujar el código de barras
                    ctx.DrawImage(barcode.ToImage(), new SixLabors.ImageSharp.Point(0, 0), 1);

                    // Dibujar el texto debajo del código de barras
                    ctx.DrawText(id, font, SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.PointF(10, barcode.Height + 5));
                });

                using (var ms = new MemoryStream())
                {
                    finalImg.Save(ms, new PngEncoder());
                    b64 = Convert.ToBase64String(ms.ToArray());
                }
            }

            return b64;
        }

        public static T PasarAMayusculas<T>(T entidad)
        {
            var props = entidad.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in props)
            {
                if (p.PropertyType == typeof(string) && p.CanWrite)
                {
                    string? valor = p.GetValue(entidad) as string; // Use safe casting to handle potential null values
                    if (!string.IsNullOrEmpty(valor))
                    {
                        valor = valor.ToUpper();
                    }
                    p.SetValue(entidad, valor);
                }
            }

            return entidad;
        }
    }
}
