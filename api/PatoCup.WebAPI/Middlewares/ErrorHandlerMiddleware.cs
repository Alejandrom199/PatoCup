using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PatoCup.Application.Exceptions;
using PatoCup.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace PatoCup.WebAPI.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                var responseModel = new Response<string>() 
                {
                    Succeeded = false,
                    Message = error.Message
                };

                string sourceLayer = "Desconocido";
                string sourceClass = "Global";
                string sourceMethod = "Desconocido";

                var stackTrace = new StackTrace(error, true);
                var frame = stackTrace.GetFrame(0);

                if (frame != null)
                {
                    var method = frame.GetMethod();
                    if (method != null && method.DeclaringType != null)
                    {
                        var declaringType = method.DeclaringType;
                        sourceLayer = declaringType.Assembly.GetName().Name!;

                        if (declaringType.Name.StartsWith("<") && declaringType.Name.Contains(">"))
                        {
                            sourceClass = declaringType.DeclaringType?.Name ?? declaringType.Name;
                            int start = declaringType.Name.IndexOf('<') + 1;
                            int end = declaringType.Name.IndexOf('>');
                            sourceMethod = declaringType.Name.Substring(start, end - start);
                        }
                        else
                        {
                            sourceClass = declaringType.Name;
                            sourceMethod = method.Name;
                        }
                    }
                }

                _logger.LogError(error,
                    "ERROR: CAPA: {Layer}, CLASE: {Class}, MÉTODO: {Method}, MENSAJE: {Message}",
                    sourceLayer, sourceClass, sourceMethod, error.Message);

                switch (error)
                {
                    case ValidationException e:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        responseModel.Errors = e.Errors.Select(f => f.ErrorMessage).ToList();
                        responseModel.Message = "Error de validación";
                        break;

                    case KeyNotFoundException:
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;

                    case UnauthorizedAccessException:
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;

                    case ApiException e:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        responseModel.Message = e.Message;
                        break;

                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        responseModel.Message = _env.IsDevelopment() ? error.Message : "Ocurrió un error interno.";
                        break;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var result = JsonSerializer.Serialize(responseModel, options);
                await response.WriteAsync(result);
            }
        }
    }
}