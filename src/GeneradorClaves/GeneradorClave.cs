using GeneradorClaves.Modelo;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorClaves
{
    internal class GeneradorClave : IGeneradorClave
    {
        private readonly RSA rsa;
        private readonly AppSettings _settings;
        public GeneradorClave(IOptions<AppSettings> options)
        {
            rsa = new RSACryptoServiceProvider();
            _settings = options.Value;
        }

        private byte[] CrearClavePrivada()
        {
            var xml= rsa.ToXmlString(true);
            return Encoding.ASCII.GetBytes(xml);
        }

        private byte[] CrearClavePublica()
        {
            var xml =  rsa.ToXmlString(false);
            return Encoding.ASCII.GetBytes(xml);
        }

        public void Inicia()
        {
            //se pocede a generar las claves
            FileStream fspu =new(Path.Combine(_settings.RutaClaves,"PublicKey.xml"),FileMode.Create,FileAccess.Write);

            byte[] publicByte = CrearClavePublica();
            fspu.Write(publicByte,0,publicByte.Length);
            fspu.Flush();

            FileStream fspr = new(Path.Combine(_settings.RutaClaves, "PrivateKey.xml"), FileMode.Create, FileAccess.Write);

            byte[] privateByte = CrearClavePrivada();
            fspr.Write(privateByte, 0, privateByte.Length);
            fspr.Flush();

            TestDeRSA();
        }

        public void TestDeRSA()
        {
            MemoryStream ms = new();

            string texto = "0002-123456,información de la ostia,Encriptar RSA en C#";
            Console.WriteLine(texto);
            //texto en byte[]
            byte[] textoByte = Encoding.UTF8.GetBytes(texto);

            //1 - se procede a generar una nueva instancia de RSA a la cual se le cargará la clave Publica y encriptará el texto.
            RSA rsa = new RSACryptoServiceProvider(1024);
            Stream fse = File.OpenRead(Path.Combine(_settings.RutaClaves, "PublicKey.xml"));
            string xmle = new StreamReader(fse).ReadToEnd();
            rsa.FromXmlString(xmle);
            
            byte[] encrypt = rsa.Encrypt(textoByte,RSAEncryptionPadding.Pkcs1);

            Console.WriteLine("Texto encriptado:");
            Console.WriteLine(Convert.ToBase64String(encrypt));
            
            ////2 - se procede a generar una nueva instancia de RSA a la cual se le cargará la clave Privada y se validará el mensaje encriptado.

            RSA des = new RSACryptoServiceProvider(1024);
            Stream fsd = File.OpenRead(Path.Combine(_settings.RutaClaves, "PrivateKey.xml"));
            string xmld = new StreamReader(fsd).ReadToEnd();
            des.FromXmlString(xmld);

            byte[] desencr = des.Decrypt(encrypt, RSAEncryptionPadding.Pkcs1);
            Console.WriteLine("Texto desencriptado:");
            Console.WriteLine(Encoding.UTF8.GetString(desencr));
        }
    }
}
