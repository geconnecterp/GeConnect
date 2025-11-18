using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.ViewModels;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class Servicio<T> : IServicio<T> where T : Dto
    {
        public readonly AppSettings _appSettings;
        protected readonly ILogger _logger;
        private string _rutaEntidad;

        public Servicio(IOptions<AppSettings> options, ILogger logger, string rutaEntidad)
        {
            _appSettings = options.Value;
            _logger = logger;
            _rutaEntidad = rutaEntidad;
        }

        public Servicio(IOptions<AppSettings> options, ILogger logger)
        {
            _appSettings = options.Value;
            _logger = logger;
            _rutaEntidad = string.Empty;
        }

        public virtual async Task<(List<T>, MetadataGrid)> BuscarAsync(string? token)
        {
            ValidaToken(token);
            ApiResponse<List<T>> respuesta;
            HttpClient client;
            HelperAPI helperAPI = new HelperAPI();
            _logger.LogInformation($"Buscando todas los coeficientes '{_rutaEntidad}'");
            if (!string.IsNullOrWhiteSpace(token))
            {
                client = helperAPI.InicializaCliente(token);
            }
            else
            {
                throw new NegocioException("Hay un problema al intentar generar la conexión. JWT.");
            }
            HttpResponseMessage response = client.GetAsync($"{_appSettings.RutaBase}{_rutaEntidad}").Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                //_logger.Log(TraceEventType.Information, $"String Response: {stringData}");
                respuesta = JsonConvert.DeserializeObject<ApiResponse<List<T>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                return (respuesta.Data ?? [], respuesta.Meta??new());
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
            }
            else
            {
                //string stringData = await response.Content.ReadAsStringAsync();
                //ErrorExceptionValidation resp = JsonSerializer.Deserialize<ErrorExceptionValidation>(stringData);
                //var error = resp.Error.First();
                //throw new NegocioException($"Código: {response.StatusCode} - Error: {error.Detail}");
                await ParseoError(response);
                return ([], new());
            }
        }

        private static void ValidaToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
            }
        }

        public virtual async Task<(List<T>, MetadataGrid)> BuscarAsync(QueryFilters filters, string token)
        {
            ValidaToken(token);

            ApiResponse<List<T>> respuesta;
            try
            {
                HelperAPI helperAPI = new HelperAPI();
                HttpClient client = helperAPI.InicializaCliente(token);

                HttpResponseMessage response;
                if (filters.Todo)
                {
                    response = await client.GetAsync($"{_appSettings.RutaBase}{_rutaEntidad}");
                    //_logger.LogInformation($"Response: {JsonConvert.SerializeObject(response)}");
                }
                else
                {
                    var link = $"{_appSettings.RutaBase}{_rutaEntidad}?{EvaluarQueryFilter(filters)}";

                    response = await client.GetAsync(link);
                    //_logger.LogInformation($"Response: {JsonConvert.SerializeObject(response)}");
                }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    //_logger.Log(TraceEventType.Information, $"String Response: {stringData}");
                    respuesta = JsonConvert.DeserializeObject<ApiResponse<List<T>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                    return (respuesta.Data ?? [], respuesta.Meta??new());
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
                }
                else
                {
                    //string stringData = await response.Content.ReadAsStringAsync();
                    //ErrorExceptionValidation resp = JsonSerializer.Deserialize<ErrorExceptionValidation>(stringData);
                    //var error = resp.Error.First();
                    //throw new NegocioException($"Código: {response.StatusCode} - Error: {error.Detail}");
                    await ParseoError(response);
                    return ([],new());
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al Buscar");
                throw;
            }
        }
       
        public virtual async Task<T> BuscarAsync(object id, string token)
        {
            ValidaToken(token);

            ApiResponse<T> respuesta;
            try
            {
                HelperAPI helperAPI = new HelperAPI();
                _logger.LogInformation($"Buscando todos los Items '{_rutaEntidad}'");
                HttpClient client = helperAPI.InicializaCliente(token);

                HttpResponseMessage response;
                //string link = $"{appSettings.RutaBase}{_rutaEntidad}?id={id}";
                string link = $"{_appSettings.RutaBase}{_rutaEntidad}/{id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    //_logger.Log(TraceEventType.Information, $"String Response: {stringData}");
                    respuesta = JsonConvert.DeserializeObject<ApiResponse<T>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                    return respuesta.Data;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
                }
                else
                {
                    //string stringData = await response.Content.ReadAsStringAsync();
                    //ErrorExceptionValidation resp = JsonSerializer.Deserialize<ErrorExceptionValidation>(stringData);
                    //var error = resp.Error.First();
                    //throw new NegocioException($"Código: {response.StatusCode} - Error: {error.Detail}");
                    await ParseoError(response);  
                    throw new NegocioException($"Código: {response.StatusCode} - Error: Hubo un error. Verifique logs.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al Buscar");
                throw;
            }
        }
        public virtual async Task<T> BuscarUnoAsync(string token)
        {
            ValidaToken(token);

            ApiResponse<T> respuesta;
            try
            {
                HelperAPI helperAPI = new HelperAPI();
                _logger.LogInformation($"Buscando todos los Items '{_rutaEntidad}'");
                HttpClient client = helperAPI.InicializaCliente(token); ;

                HttpResponseMessage response;
                string link = $"{_appSettings.RutaBase}{_rutaEntidad}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    //_logger.Log(TraceEventType.Information, $"String Response: {stringData}");
                    respuesta = JsonConvert.DeserializeObject<ApiResponse<T>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                    return respuesta.Data;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
                }
                else
                {
                    //string stringData = await response.Content.ReadAsStringAsync();
                    //ErrorExceptionValidation resp = JsonSerializer.Deserialize<ErrorExceptionValidation>(stringData);
                    //var error = resp.Error.First();
                    //throw new NegocioException($"Código: {response.StatusCode} - Error: {error.Detail}");
                    await ParseoError(response);
                    throw new NegocioException($"Código: {response.StatusCode} - Error: Hubo un error. Verifique logs.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al Buscar Uno");
                throw;
            }
        }


        public string EvaluarEntidad4Link<S>(S entidad) where S : class
        {
            bool first = true;
            string cadena = string.Empty;
            foreach (var prop in entidad.GetType().GetProperties())
            {
                var valor = prop.GetValue(entidad, null);
                if (prop.PropertyType == typeof(int) && valor != null)
                {
                    if ((int)valor != 0)
                    {
                        ComponeCadena(ref first, ref cadena, prop, valor);
                        continue;
                    }
                }
                if (prop.PropertyType == typeof(Nullable<int>))
                {
                    if (((int?)valor).HasValue)
                    {
                        ComponeCadena(ref first, ref cadena, prop, valor);
                        continue;
                    }
                }


                if (prop.PropertyType == typeof(decimal) && valor != null)
                {
                    if ((decimal)valor != 0)
                    {
                        ComponeCadena(ref first, ref cadena, prop, valor);
                        continue;

                    }
                }
                if (prop.PropertyType == typeof(Nullable<decimal>))
                {
                    if (((decimal?)valor).HasValue)
                    {
                        ComponeCadena(ref first, ref cadena, prop, valor);
                        continue;

                    }
                }

                if (prop.PropertyType == typeof(string) && valor != null)
                {
                    if (!string.IsNullOrWhiteSpace((string)valor))
                    {
                        ComponeCadena(ref first, ref cadena, prop, valor);
                        continue;

                    }
                }

                if (prop.PropertyType == typeof(Nullable<DateTime>))
                {
                    if (((DateTime?)valor).HasValue)
                    {
                        ComponeCadena(ref first, ref cadena, prop, valor);
                        continue;

                    }
                }

                if (valor != null)
                {
                    ComponeCadena(ref first, ref cadena, prop, valor);
                    continue;
                }

            }

            return cadena;
        }
        public string EvaluarQueryFilter(QueryFilters filters)
        {
            bool first = true;
            string cadena = string.Empty;
            foreach (var prop in filters.GetType().GetProperties())
            {
                //no evaluo la propiedad Todo
                if (prop.Name.Equals("Todo"))
                { continue; }
                var valor = prop.GetValue(filters, null);
                if (valor == null)
                {
                    continue;
                }
                if (prop.PropertyType == typeof(int))
                {
                    if ((int)valor != 0)
                    {
                        ComponeCadena(ref first, ref cadena, prop, valor);
                        continue;
                    }
                }

                if (prop.PropertyType == typeof(string))
                {
                    if (!string.IsNullOrWhiteSpace((string)valor))
                    {
                        ComponeCadena(ref first, ref cadena, prop, valor);
                        continue;

                    }
                }

                if (prop.PropertyType == typeof(Nullable<DateTime>))
                {
                    if (((DateTime?)valor).HasValue)
                    {
                        ComponeCadena(ref first, ref cadena, prop, ((DateTime?)valor).Value);
                        continue;

                    }
                }

                if (valor != null)
                {
                    ComponeCadena(ref first, ref cadena, prop, valor);
                    continue;
                }

            }

            return cadena;

        }
        //public string EvaluarQueryFilter(ProductFilters filters)
        //{
        //    bool first = true;
        //    string cadena = string.Empty;
        //    foreach (var prop in filters.GetType().GetProperties())
        //    {
        //        //no evaluo la propiedad Todo
        //        if (prop.Name.Equals("Todo"))
        //        { continue; }
        //        var valor = prop.GetValue(filters, null);
        //        if (prop.PropertyType == typeof(int))
        //        {
        //            if ((int)valor != 0)
        //            {
        //                ComponeCadena(ref first, ref cadena, prop, valor);
        //            }
        //            continue;
        //        }

        //        if (prop.PropertyType == typeof(string))
        //        {
        //            if (!string.IsNullOrWhiteSpace((string)valor))
        //            {
        //                ComponeCadena(ref first, ref cadena, prop, valor);

        //            }
        //            continue;
        //        }

        //        if (prop.PropertyType == typeof(int?))
        //        {
        //            if (!string.IsNullOrWhiteSpace((string)valor))
        //            {
        //                ComponeCadena(ref first, ref cadena, prop, valor);

        //            }
        //            continue;
        //        }

        //        if (prop.PropertyType == typeof(long))
        //        {
        //            if (!string.IsNullOrWhiteSpace((string)valor))
        //            {
        //                ComponeCadena(ref first, ref cadena, prop, valor);

        //            }
        //            continue;
        //        }

        //        if (prop.PropertyType == typeof(long?))
        //        {
        //            if (((DateTime?)valor).HasValue)
        //            {
        //                ComponeCadena(ref first, ref cadena, prop, valor);

        //            }
        //            continue;
        //        }

        //        if (prop.PropertyType == typeof(DateTime?))
        //        {
        //            if (((DateTime?)valor).HasValue)
        //            {
        //                ComponeCadena(ref first, ref cadena, prop, valor);

        //            }
        //            continue;
        //        }

        //        if (valor != null)
        //        {
        //            ComponeCadena(ref first, ref cadena, prop, valor);
        //            continue;
        //        }

        //    }

        //    return cadena;

        //}

        private static void ComponeCadena(ref bool first, ref string cadena, PropertyInfo prop, object? valor)
        {
            if (first) { first = false; }
            else { cadena += "&"; }
            cadena += $"{prop.Name}={valor}";
        }

        public virtual async Task<bool> AgregarAsync(T entidad, string token)
        {
            ValidaToken(token);

            ApiResponse<bool> respuesta;
            try
            {
                HelperAPI helperAPI = new HelperAPI();
                _logger.LogInformation("Agregando los datos de la entidad.");

                HttpClient client = helperAPI.InicializaCliente(entidad, token, out StringContent content);
                client.BaseAddress = new Uri(_appSettings.RutaBase??"");
                HttpResponseMessage response;
                string link = $"{_rutaEntidad}";
                response = await client.PostAsync(link, content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"stringData: {stringData}");

                    respuesta = JsonConvert.DeserializeObject<ApiResponse<bool>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                    return respuesta.Data;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
                }
                else
                {
                    await ParseoError(response);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al Agregar");
                throw;

            }
        }

        protected async Task ParseoError(HttpResponseMessage response)
        {
            string stringData = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"stringData: {stringData}");
            ErrorExceptionValidation resp = JsonConvert.DeserializeObject<ErrorExceptionValidation>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
            if (resp.Error != null)
            {
                var error = resp.Error.First();
                throw new NegocioException($"Código: {response.StatusCode} - Error: {error.Detail}");
            }
            else
            {
                var errores = ParseoErroresFluentValidate(stringData);
                string msj = string.Empty;
                foreach (var e in errores)
                {
                    var titulo = e.Titulo;
                    foreach (var m in e.Mensajes)
                    {
                        msj += $"{e.Titulo} - {m}\n";
                    }

                }
                throw new NegocioException($"Error(es): {msj} ");
            }
        }

        public virtual async Task<bool> ActualizarAsync(object id, T entidad, string token)
        {
            ValidaToken(token);

            ApiResponse<bool> respuesta;
            try
            {
                HelperAPI helperAPI = new HelperAPI();
                _logger.LogInformation("Actualizando los datos de la entidad");
                HttpClient client = helperAPI.InicializaCliente(entidad, token, out StringContent content);
                client.BaseAddress = new Uri(_appSettings.RutaBase ?? "");

                var link = $"{_rutaEntidad}/{id}";

                HttpResponseMessage response = await client.PutAsync(link, content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"String Response: {stringData}");
                    respuesta = JsonConvert.DeserializeObject<ApiResponse<bool>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                    return respuesta.Data;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"stringData: {stringData}");

                    if (!string.IsNullOrWhiteSpace(stringData))
                    {
                        ErrorExceptionValidation resp = JsonConvert.DeserializeObject<ErrorExceptionValidation>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                        if (resp.Error != null)
                        {
                            var error = resp.Error.First();
                            throw new NegocioException($"Código: {response.StatusCode} - Error: {error.Detail}");
                        }
                        else
                        {
                            var errores = ParseoErroresFluentValidate(stringData);
                            string msj = string.Empty;
                            foreach (var e in errores)
                            {
                                var titulo = e.Titulo;
                                foreach (var m in e.Mensajes)
                                {
                                    msj += $"{e.Titulo} - {m}\n";
                                }

                            }
                            throw new NegocioException($"Error(es): {msj} ");
                        }
                    }
                    else
                    {
                        throw new NegocioException($"Código: {response.StatusCode} - Error: Hubo un error. Verifique logs.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al Actualizar");
                throw;
            }
        }

        private List<ErroresFluentValidatorVM> ParseoErroresFluentValidate(string stringData)
        {
            var errores = new List<ErroresFluentValidatorVM>();
            var data = stringData.Split(new char[] { '{', '}' })[2];
            var errs = data.Split(new string[] { "]," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in errs)
            {
                var e = item.Split(':');
                var reg = new ErroresFluentValidatorVM();
                reg.Titulo = e[0].Replace("\"", "").Replace("\\", "");
                var msgs = e[1].Split(',');
                reg.Mensajes = new List<string>();
                foreach (var m in msgs)
                {
                    reg.Mensajes.Add(m.Replace("\"", "").Replace("\\", "").Replace("[", "").Replace("]", ""));
                }

                errores.Add(reg);
            }


            return errores;

        }

        public virtual async Task<bool> EliminarAsync(object id, string token)
        {
            ValidaToken(token);

            ApiResponse<bool> respuesta;
            try
            {
                HelperAPI helperAPI = new HelperAPI();
                _logger.LogInformation($"Eliminando datos. Id:{id}");
                HttpClient client = helperAPI.InicializaCliente(token);
                client.BaseAddress = new Uri(_appSettings.RutaBase ?? "");
                HttpResponseMessage response;
                var link = $"{_rutaEntidad}/{id}";

                response = await client.DeleteAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"String Response: {stringData}");
                    respuesta = JsonConvert.DeserializeObject<ApiResponse<bool>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                    return respuesta.Data;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
                }
                else
                {
                    //string stringData = await response.Content.ReadAsStringAsync();
                    //_logger.Log(TraceEventType.Information, $"stringData: {stringData}");
                    //if (!string.IsNullOrWhiteSpace(stringData))
                    //{
                    //    ErrorExceptionValidation resp = JsonSerializer.Deserialize<ErrorExceptionValidation>(stringData);
                    //    var error = resp.Error.First();
                    //    throw new NegocioException($"Código: {response.StatusCode} - Error: {error.Detail}");
                    //}
                    //else
                    //{
                    //    throw new NegocioException($"Código: {response.StatusCode} - Error: Hubo un error. Verifique logs.");
                    //}await ParseoError(response);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al Eliminar");
                throw;
            }
        }

        // Helpers genéricos para minimizar código y asignaciones
        internal async Task<RespuestaGenerica<TDto>> GetListaAsync<TDto>(string url, string token, string mensajeError)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                using var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                    return new RespuestaGenerica<TDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };
                }
                else
                {
                    var msg = await ReadApiErrorAsync(response);
                    _logger.LogWarning($"Algo no fue bien. Error de API {msg}");
                    return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = mensajeError };
            }
        }

        internal static async Task<string> ReadApiErrorAsync(HttpResponseMessage response)
        {
            var raw = await response.Content.ReadAsStringAsync();
            try
            {
                var err = JsonConvert.DeserializeObject<ExceptionValidation>(raw);
                return err?.Detail ?? raw;
            }
            catch
            {
                return string.IsNullOrWhiteSpace(raw) ? "Error desconocido en la API" : raw;
            }
        }

    }
}
