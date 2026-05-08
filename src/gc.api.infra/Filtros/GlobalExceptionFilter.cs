namespace gc.api.infra.Filtros
{
    using gc.infraestructura.Core.EntidadesComunes;
    using gc.infraestructura.Core.Exceptions;
    using gc.infraestructura.Core.Extensions;
    using gc.infraestructura.Core.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Net;

    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly IExceptionManager _ex;
        private readonly ILogger<GlobalExceptionFilter> _logger;
        public GlobalExceptionFilter(IExceptionManager exceptionManager, ILogger<GlobalExceptionFilter> logger)
        {
            _ex = exceptionManager;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(new ExceptionFormatterExtension(context.Exception).GetValue());
            Exception exception = _ex.HandleException(context.Exception);
            ExceptionValidation validation;

            if (exception is NegocioException)
            {
                validation = new ExceptionValidation
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Bad Request",
                    Detail = exception.Message,
                    TypeException = "NegocioException"
                };

                context.Result = new BadRequestObjectResult(new { error = new[] { validation } });
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.ExceptionHandled = true;
            }            
            else if (exception is NotFoundException)
            {

                validation = new ExceptionValidation
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Not Found",
                    Detail = exception.Message,
                    TypeException = "NotFoundException"

                };


                context.Result = new NotFoundObjectResult(new { error = new[] { validation } });
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.ExceptionHandled = true;
            }
            else if (exception is SecurityException)
            {

                validation = new ExceptionValidation
                {
                    Status = (int)HttpStatusCode.Forbidden,
                    Title = "Not Found",
                    Detail = exception.Message,
                    TypeException = "SecurityException"

                };


                context.Result = new ObjectResult(new { error = new[] { validation } });
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.ExceptionHandled = true;
            }
            else if (exception is SqlException sqlException)
            {
                if (sqlException.Number == 2627 || sqlException.Number == 2601)
                {
                    validation = new ExceptionValidation
                    {
                        Status = (int)HttpStatusCode.Conflict,
                        Title = "Conflict",
                        Detail = $"Ya existe un registro con la misma clave. Detalle técnico: {sqlException.Message}",
                        TypeException = "SqlDuplicateKeyException"
                    };

                    context.Result = new ConflictObjectResult(new { error = new[] { validation } });
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    context.ExceptionHandled = true;
                }
                else
                {
                    validation = new ExceptionValidation
                    {
                        Status = (int)HttpStatusCode.InternalServerError,
                        Title = "Database Error",
                        Detail = $"Ocurrió un error al acceder a la base de datos. Detalle técnico: {sqlException.Message}",
                        TypeException = "SqlException"
                    };

                    context.Result = new ObjectResult(new { error = new[] { validation } })
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.ExceptionHandled = true;
                }
            }
            else
            {
                validation = new ExceptionValidation
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = "Conflict",
                    Detail = $"{exception.Message} Si el mismo persiste, avise al Administrador del Sistema.",
                    TypeException = "Exception"

                };

                context.Result = new ConflictObjectResult(new { error = new[] { validation } });
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
                context.ExceptionHandled = true;
            }
        }
    }
}

