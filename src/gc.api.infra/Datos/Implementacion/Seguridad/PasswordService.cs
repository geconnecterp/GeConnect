
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Servicios;
using gc.api.infra.Datos.Contratos.Security;
using gc.infraestructura.Constantes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Schema;

namespace gc.api.infra.Datos.Implementacion.Security
{
    public class PasswordService :Servicio<RegistroUserDto>, IPasswordService
    {
        private readonly PasswordOptions _options;

        public PasswordService(IUnitOfWork uow, IOptions<PasswordOptions> options):base(uow)
        {
            _options = options.Value;
        }

        public bool Check(string hash,string usuario, string password)
        {
            if(hash == null || usuario == null || password == null)
            {
                return false;
            }

            //se invoca la encriptación para comparar la con el hash
            var patron = new { usuario, clave = password };

            string sp = $"select {ConstantesGC.StoredProcedures.FX_PASSWORD_ENCRIPTA}('{JsonSerializer.Serialize(patron)}')";
            var pass = _repository.InvokarSpScalar(sp, null,false,true,false);

            if (pass != null)
            {
                //se comparar los resultados
                if (hash.Equals(pass.ToString()))
                {
                    return true;
                }
            }
            return false ;
            //var parts = hash.Split('.');
            //if (parts.Length != 3)
            //{
            //    throw new FormatException("Expected Hash Format");
            //}

            //var iterations = Convert.ToInt32(parts[0]);
            //var salt = Convert.FromBase64String(parts[1]);
            //var key = Convert.FromBase64String(parts[2]);

            //using (var alg = new PasswordDeriveBytes(password, salt, "SHA256", iterations))
            //{
            //    var keyToCheck = alg.GetBytes(_options.KeySize);

            //    return keyToCheck.SequenceEqual(key);
            //}
        }

        public string CalculaClave(RegistroUserDto registroUserDto)
        {
            #region Este código corresponde a la encriptación de la clave de acceso
            //            //_options.Salt nunca deberia ser nula. Al tomarlo de la configuración se deberia hidratar transparentente.
            //#pragma warning disable CS8604 // Posible argumento de referencia nulo
            //            var saltOpt = Encoding.ASCII.GetBytes(_options.Salt);
            //#pragma warning restore CS8604 // Posible argumento de referencia nulo
            //                              //PBKDF2 IMPLELMETACION
            //            using (var alg = new PasswordDeriveBytes(password, saltOpt, "SHA256", _options.Iterations))
            //            {               
            //                var key = Convert.ToBase64String(alg.GetBytes(_options.KeySize));
            //#pragma warning disable CS8604 // Posible argumento de referencia nulo
            //                var salt = Convert.ToBase64String(alg.Salt);
            //#pragma warning restore CS8604 // Posible argumento de referencia nulo
            //                return $"{_options.Iterations}.{salt}.{key}";
            #endregion

            if (registroUserDto == null)
            {
                throw new SecurityException("No se recepcionaron los datos de registración.");
            }    

            var patron= new {usuario=registroUserDto.User,clave=registroUserDto.Password};

            string sp = $"select {ConstantesGC.StoredProcedures.FX_PASSWORD_ENCRIPTA}('{JsonSerializer.Serialize(patron)}')";
            var pass = _repository.InvokarSpScalar(sp, null,false,true,false);
            if(pass != null)
            {
                return pass.ToString();
            }
            else
            {
                throw new SecurityException("No se pudo generar la clave");
            }
        }
    
    }
}

