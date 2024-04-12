﻿using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.ViewModels;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class Servicio<T>:IServicio<T> where T : Dto
    {
        private readonly AppSettings appSettings;
        private readonly ILoggerHelper _logger;
        private string _rutaEntidad;

        public Servicio(IOptions<AppSettings> options, ILoggerHelper logger, string rutaEntidad)
        {
            appSettings = options.Value;
            _logger = logger;
            _rutaEntidad = rutaEntidad;
        }

        public Servicio(IOptions<AppSettings> options, ILoggerHelper logger)
        {
            appSettings = options.Value;
            _logger = logger;
            _rutaEntidad = string.Empty;
        }
        public virtual async Task<(List<T>?, Metadata?)> BuscarAsync(string? token)
        {
            ValidaToken(token);
            ApiResponse<List<T>> respuesta;
            HttpClient client;
            HelperAPI helperAPI = new HelperAPI();
            _logger.Log(TraceEventType.Information, $"Buscando todas los coeficientes '{_rutaEntidad}'");
            if (!string.IsNullOrWhiteSpace(token))
            {
                client = helperAPI.InicializaCliente(token);
            }
            else { throw new NegocioException("Hay un problema al intentar generar la conexión. JWT."); 
            }
            HttpResponseMessage response = client.GetAsync($"{appSettings.RutaBase}{_rutaEntidad}").Result;
            _logger.Log(TraceEventType.Information, $"Response: {JsonSerializer.Serialize(response)}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                //_logger.Log(TraceEventType.Information, $"String Response: {stringData}");
                respuesta = JsonSerializer.Deserialize<ApiResponse<List<T>>>(stringData);
                return (respuesta.Data, respuesta.Meta);
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
                return (null, null);
            }
        }

        private static void ValidaToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
            }
        }

        public virtual async Task<(List<T>?, Metadata?)> BuscarAsync(QueryFilters filters, string? token)
        {
            ValidaToken(token);

            ApiResponse<List<T>> respuesta;
            try
            {
                HelperAPI helperAPI = new HelperAPI();
                _logger.Log(TraceEventType.Information, $"Buscando todos los Items '{_rutaEntidad}' - Filtro: {JsonSerializer.Serialize(filters)}");
                HttpClient client = helperAPI.InicializaCliente(token);

                HttpResponseMessage response;
                if (filters.Todo)
                {
                    response = await client.GetAsync($"{appSettings.RutaBase}{_rutaEntidad}");
                    _logger.Log(TraceEventType.Information, $"Response: {JsonSerializer.Serialize(response)}");
                }
                else
                {
                    var link = $"{appSettings.RutaBase}{_rutaEntidad}?{EvaluarQueryFilter(filters)}";

                    response = await client.GetAsync(link);
                    _logger.Log(TraceEventType.Information, $"Response: {JsonSerializer.Serialize(response)}");
                }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    //_logger.Log(TraceEventType.Information, $"String Response: {stringData}");
                    respuesta = JsonSerializer.Deserialize<ApiResponse<List<T>>>(stringData);
                    return (respuesta.Data, respuesta.Meta);
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
                    return (null, null);
                }

            }
            catch (Exception ex)
            {
                throw _logger.Log(ex);
            }
        }
        //public virtual async Task<(List<T>, Metadata)> BuscarAsync(ProductFilters filters, string token)
        //{
        //    ValidaToken(token);

        //    ApiResponse<List<T>> respuesta;
        //    try
        //    {
        //        HelperAPI helperAPI = new HelperAPI();
        //        _logger.Log(TraceEventType.Information, $"Buscando todos los Items '{_rutaEntidad}' - Filtro: {JsonSerializer.Serialize(filters)}");
        //        HttpClient client = helperAPI.InicializaCliente(token);

        //        HttpResponseMessage response;
        //        if (filters.Todo)
        //        {
        //            response = await client.GetAsync($"{appSettings.RutaBase}{_rutaEntidad}");
        //            _logger.Log(TraceEventType.Information, $"Response: {JsonSerializer.Serialize(response)}");
        //        }
        //        else
        //        {
        //            var link = $"{appSettings.RutaBase}{_rutaEntidad}?{EvaluarQueryFilter(filters)}";

        //            response = await client.GetAsync(link);
        //            _logger.Log(TraceEventType.Information, $"Response: {JsonSerializer.Serialize(response)}");
        //        }

        //        if (response.StatusCode == HttpStatusCode.OK)
        //        {
        //            string stringData = await response.Content.ReadAsStringAsync();
        //            //_logger.Log(TraceEventType.Information, $"String Response: {stringData}");
        //            respuesta = JsonSerializer.Deserialize<ApiResponse<List<T>>>(stringData);
        //            return (respuesta.Data, respuesta.Meta);
        //        }
        //        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        //        {
        //            throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
        //        }
        //        else
        //        {
        //            //string stringData = await response.Content.ReadAsStringAsync();
        //            //ErrorExceptionValidation resp = JsonSerializer.Deserialize<ErrorExceptionValidation>(stringData);
        //            //var error = resp.Error.First();
        //            //throw new NegocioException($"Código: {response.StatusCode} - Error: {error.Detail}");
        //            await ParseoError(response);
        //            return (null, null);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw _logger.Log(ex);
        //    }
        //}

        public virtual async Task<T> BuscarAsync(object id, string token)
        {
            ValidaToken(token);

            ApiResponse<T> respuesta;
            try
            {
                HelperAPI helperAPI = new HelperAPI();
                _logger.Log(TraceEventType.Information, $"Buscando todos los Items '{_rutaEntidad}'");
                HttpClient client = helperAPI.InicializaCliente(token);

                HttpResponseMessage response;
                //string link = $"{appSettings.RutaBase}{_rutaEntidad}?id={id}";
                string link = $"{appSettings.RutaBase}{_rutaEntidad}/{id}";

                response = await client.GetAsync(link);
                _logger.Log(TraceEventType.Information, $"Response: {JsonSerializer.Serialize(response)}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    //_logger.Log(TraceEventType.Information, $"String Response: {stringData}");
                    respuesta = JsonSerializer.Deserialize<ApiResponse<T>>(stringData);
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
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw _logger.Log(ex);
            }
        }
        public virtual async Task<T> BuscarUnoAsync(string token)
        {
            ValidaToken(token);

            ApiResponse<T> respuesta;
            try
            {
                HelperAPI helperAPI = new HelperAPI();
                _logger.Log(TraceEventType.Information, $"Buscando todos los Items '{_rutaEntidad}'");
                HttpClient client = helperAPI.InicializaCliente(token); ;

                HttpResponseMessage response;
                string link = $"{appSettings.RutaBase}{_rutaEntidad}";

                response = await client.GetAsync(link);
                _logger.Log(TraceEventType.Information, $"Response: {JsonSerializer.Serialize(response)}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    //_logger.Log(TraceEventType.Information, $"String Response: {stringData}");
                    respuesta = JsonSerializer.Deserialize<ApiResponse<T>>(stringData);
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
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw _logger.Log(ex);
            }
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

        private static void ComponeCadena(ref bool first, ref string cadena, PropertyInfo prop, object valor)
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
                _logger.Log(TraceEventType.Information, "Agregando los datos de la entidad.");
                _logger.Log(TraceEventType.Information, $"A Agregar: {JsonSerializer.Serialize(entidad)}");

                HttpClient client = helperAPI.InicializaCliente(entidad, token, out StringContent content);
                client.BaseAddress = new Uri(appSettings.RutaBase);
                HttpResponseMessage response;
                string link = $"{_rutaEntidad}";
                response = await client.PostAsync(link, content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.Log(TraceEventType.Information, $"stringData: {stringData}");

                    respuesta = JsonSerializer.Deserialize<ApiResponse<bool>>(stringData);
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
                throw _logger.Log(ex);
            }
        }

        protected async Task ParseoError(HttpResponseMessage response)
        {
            string stringData = await response.Content.ReadAsStringAsync();
            _logger.Log(TraceEventType.Information, $"stringData: {stringData}");
            ErrorExceptionValidation resp = JsonSerializer.Deserialize<ErrorExceptionValidation>(stringData);
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
                _logger.Log(TraceEventType.Information, "Actualizando los datos de la entidad");
                _logger.Log(TraceEventType.Information, $"A Modificar: {JsonSerializer.Serialize(entidad)}");
                HttpClient client = helperAPI.InicializaCliente(entidad, token, out StringContent content);
                client.BaseAddress = new Uri(appSettings.RutaBase);

                var link = $"{_rutaEntidad}/{id}";

                HttpResponseMessage response = await client.PutAsync(link, content);
                _logger.Log(TraceEventType.Information, $"Response: {JsonSerializer.Serialize(response)}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.Log(TraceEventType.Information, $"String Response: {stringData}");
                    respuesta = JsonSerializer.Deserialize<ApiResponse<bool>>(stringData);
                    return respuesta.Data;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.Log(TraceEventType.Information, $"stringData: {stringData}");

                    if (!string.IsNullOrWhiteSpace(stringData))
                    {
                        ErrorExceptionValidation resp = JsonSerializer.Deserialize<ErrorExceptionValidation>(stringData);
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
                throw _logger.Log(ex);
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
                _logger.Log(TraceEventType.Information, $"Eliminando datos. Id:{id}");
                HttpClient client = helperAPI.InicializaCliente(token);
                client.BaseAddress = new Uri(appSettings.RutaBase);
                HttpResponseMessage response;
                var link = $"{_rutaEntidad}/{id}";

                response = await client.DeleteAsync(link);
                _logger.Log(TraceEventType.Information, $"Response: {JsonSerializer.Serialize(response)}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.Log(TraceEventType.Information, $"String Response: {stringData}");
                    respuesta = JsonSerializer.Deserialize<ApiResponse<bool>>(stringData);
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
                throw _logger.Log(ex);
            }
        }



    }
}
