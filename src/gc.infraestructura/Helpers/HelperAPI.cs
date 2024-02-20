using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace gc.infraestructura.Core.Helpers
{
    public class HelperAPI
    {
        public HelperAPI()
        {
        }

        public HttpClient InicializaCliente()
        {
            return InicializaCliente("");
        }

        public HttpClient InicializaCliente(string token)
        {
            //Bypass the Certificate
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };


            HttpClient client = new HttpClient(clientHandler);
            //{
            //    BaseAddress = new Uri(rutaBase)
            //};
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);

            //get IP

            //agregamos el token para la solicitud de los datos
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public HttpClient InicializaCliente<T>(T entidad, string token, out StringContent contentData)
        {
            HttpClient client = InicializaCliente(token);

            string dataJson = JsonSerializer.Serialize(entidad);
            contentData = new StringContent(dataJson, Encoding.UTF8, "application/json");
            return client;
        }

        public bool EsTokenValido(string token, out string usuario, out string role, out string email)
        {
            usuario = string.Empty;
            role = string.Empty;
            email = string.Empty;

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var tokenS = handler.ReadToken(token) as JwtSecurityToken;
                
                if (tokenS == null) { return false; }

                var ahora = DateTime.UtcNow;


                if (ahora >= tokenS.ValidFrom && ahora < tokenS.ValidTo)
                {
                    usuario = tokenS.Claims.First(c => c.Type.Contains("User")).Value;
                    role = tokenS.Claims.First(c => c.Type.Contains("role")).Value;
                    email = tokenS.Claims.First(c => c.Type.Contains("email")).Value;
                    return true;
                }
            }
            catch
            {
            }
            return false;

        }       
    }
}