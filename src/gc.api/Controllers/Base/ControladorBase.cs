using gc.infraestructura.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;

namespace gc.api.Controllers.Base
{
    public class ControladorBase : ControllerBase
    {
       

        /// <summary>
        /// Toma el token de autenticación y Autorización y devuelve el rol y el usuario en ese orden
        /// </summary>
        /// <returns>Devuelve Rol y Usuario (en ese orden)</returns>
        internal (string, string) ObtenerTokenDesdeRequestAsync(bool returnRole = true)
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            var handler = new JwtSecurityTokenHandler(); //Libreria System.IdentityModel.Token.Jwt (6.7.1)
            JwtSecurityToken? tokenS = handler.ReadToken(token) as JwtSecurityToken;

            if (tokenS != null)
            {
                var rol = tokenS.Claims.First(c => c.Type.Contains("role")).Value;
                var usuario = tokenS.Claims.First(c => c.Type.Contains("User")).Value;
                var id = tokenS.Claims.First(c => c.Type.Contains("Id")).Value;
                if (returnRole)
                {
                    return (rol, usuario);
                }
                else
                {
                    return (id, usuario);
                }
            }
            else
            {
                throw new UnauthorizedException("No se Autorizo el acceso. Debería autenticarse nuevamente.");
            }
         
        }

        internal string ObtenerIPRemota(HttpContext context)
        {
            //    //IPAddress ipRemota = context.Connection.RemoteIpAddress; //Request.HttpContext.Connection.RemoteIpAddress;
            //    //string res = string.Empty;
            //    //if (ipRemota != null)
            //    //{
            //    //    // If we got an IPV6 address, then we need to ask the network for the IPV4 address 
            //    //    // This usually only happens when the browser is on the same machine as the server.
            //    //    if(ipRemota.AddressFamily== AddressFamily.InterNetworkV6)
            //    //    {
            //    //        ipRemota = Dns.GetHostEntry(ipRemota).AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
            //    //    }
            //    //    res = ipRemota.ToString();
            //    //}
            //    //else
            //    //{
            //    //    res= Request.HttpContext.Connection.RemoteIpAddress.ToString();
            //    //};
            string? ip = GetHeaderValueAs<string>("X-ClientUsr").SplitCsv().FirstOrDefault();
            //////string ip = GetHeaderValueAs<string>("X-Forwarded-For").SplitCsv().FirstOrDefault();
            //////if(string.IsNullOrWhiteSpace(ip) && HttpContext?.Connection?.RemoteIpAddress != null)
            //////{
            //////    ip = HttpContext.Connection.RemoteIpAddress.ToString();
            //////}

            //////if (string.IsNullOrWhiteSpace(ip))
            //////{
            //////    ip = GetHeaderValueAs<string>("REMOTE_ADDR");
            //////}

            //////if (string.IsNullOrWhiteSpace(ip))
            //////{
            //////    throw new Exception("No se puede determinar la IP del cliente.");
            //////}
            //////if (!string.IsNullOrWhiteSpace(context.Request.Headers["X-Forwarded-For"]))
            //////{
            //////    ip = context.Request.Headers["X-Forwarded-For"];
            //////}
            //////else
            //////{
            ////IPAddress ipRemota = context.Request.HttpContext.Features.Get<IHttpConnectionFeature>().RemoteIpAddress;
            ////if (ipRemota != null)
            ////{
            ////    if (ipRemota.AddressFamily == AddressFamily.InterNetworkV6)
            ////    {
            ////        ipRemota = Dns.GetHostEntry(ipRemota).AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
            ////    }
            ////    ip = ipRemota.ToString();
            ////}
            ////else
            ////{
            ////    ip = context.Request.Headers["X-Forwarded-For"];
            ////}
            //////}
            //ip = context.Request.Headers["X-ClientUsr"];
            return ip;
        }

        public T GetHeaderValueAs<T>(string headerName)
        {
            StringValues values;

            if (HttpContext?.Request?.Headers?.TryGetValue(headerName, out values) ?? false)
            {
                string rawValues = values.ToString();   // writes out as Csv when there are multiple.

                if (!string.IsNullOrWhiteSpace(rawValues))
                {
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
                }

            }
            return default;
        }

    }
}
