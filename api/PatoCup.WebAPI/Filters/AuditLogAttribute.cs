using Microsoft.AspNetCore.Mvc.Filters;
using PatoCup.Application.DTOs.Audit;
using PatoCup.Application.Interfaces.Services.Audit;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

namespace PatoCup.WebAPI.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuditLogAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _actionType;

        // Palabras sensibles a ocultar
        private readonly string[] _sensitiveKeywords = new[] {
            "password", "contrasena", "contraseña", "token", "confirmPassword", "secret"
        };

        public AuditLogAttribute(string actionType)
        {
            _actionType = actionType;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            try
            {
                var auditService = context.HttpContext.RequestServices.GetService<IAuditService>();

                if (auditService != null)
                {
                    // A. Usuario
                    var userIdString = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                       ?? context.HttpContext.User.FindFirst("uid")?.Value;
                    int? userId = int.TryParse(userIdString, out var uid) ? uid : null;

                    string ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    if (ipAddress == "::1") ipAddress = "127.0.0.1";

                    string requestData = "Sin datos";
                    if (context.ActionArguments.Count > 0)
                    {
                        var safeArgs = SanitizeArguments(context.ActionArguments);
                        requestData = JsonSerializer.Serialize(safeArgs);
                    }

                    string statusPrefix = "[EXITOSO]";
                    string errorDetail = "";

                    if (resultContext.Exception != null)
                    {
                        statusPrefix = "[EXCEPCIÓN CRÍTICA]";
                        errorDetail = $" | Error: {resultContext.Exception.Message}";
                    }
                    else if (resultContext.HttpContext.Response.StatusCode >= 400)
                    {
                        statusPrefix = $"[FALLIDO {resultContext.HttpContext.Response.StatusCode}]";
                    }

                    var finalMessage = $"{statusPrefix}{errorDetail} | Datos: {requestData}";

                    var auditDto = new CreateAuditLogDto
                    {
                        UserId = userId,
                        ActionType = _actionType,
                        Message = finalMessage,
                        IPAddress = ipAddress
                    };

                    _ = auditService.LogAction(auditDto);
                }
            }
            catch
            {
            }
        }

        private Dictionary<string, object> SanitizeArguments(IDictionary<string, object?> arguments)
        {
            var safeDictionary = new Dictionary<string, object>();

            foreach (var arg in arguments)
            {
                if (arg.Value == null)
                {
                    safeDictionary.Add(arg.Key, "null");
                    continue;
                }

                var type = arg.Value.GetType();

                
                if (IsSimpleType(type))
                {
                    // Si el nombre del argumento simple es "password" (raro pero posible), lo ocultamos
                    if (_sensitiveKeywords.Any(k => arg.Key.ToLower().Contains(k)))
                        safeDictionary.Add(arg.Key, "******");
                    else
                        safeDictionary.Add(arg.Key, arg.Value);
                }
                else
                {
                    try
                    {
                        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        var cleanObj = new Dictionary<string, object>();

                        foreach (var prop in properties)
                        {
                            var propName = prop.Name;
                            var propValue = prop.GetValue(arg.Value);

                            if (_sensitiveKeywords.Any(k => propName.ToLower().Contains(k)))
                            {
                                cleanObj.Add(propName, "******");
                            }
                            else
                            {
                                cleanObj.Add(propName, propValue);
                            }
                        }
                        safeDictionary.Add(arg.Key, cleanObj);
                    }
                    catch
                    {
                        // Si falla la reflexión, guardamos el ToString por si acaso
                        safeDictionary.Add(arg.Key, arg.Value.ToString());
                    }
                }
            }

            return safeDictionary;
        }

        // Helper para detectar tipos que NO son clases complejas
        private bool IsSimpleType(Type type)
        {
            return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan)
                || type == typeof(Guid);
        }
    }
}