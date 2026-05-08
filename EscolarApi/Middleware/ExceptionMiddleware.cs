using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EscolarApi.DTOs.Response;

namespace EscolarApi.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Por defecto es 500
            int statusCode = StatusCodes.Status500InternalServerError;
            string message = "Ocurrió un error inesperado en el servidor.";

            // Lógica para detectar errores de validación de negocio
            // Si la excepción no tiene "InnerException" y nosotros escribimos el mensaje,
            // es muy probable que sea una validación de nuestro Service.
            if (exception is Exception && !string.IsNullOrEmpty(exception.Message)
                && exception.StackTrace != null && exception.StackTrace.Contains("Services"))
            {
                // Si el error viene de la capa de servicios, lo tratamos como un error del cliente (400)
                statusCode = StatusCodes.Status400BadRequest;
                message = exception.Message;
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                StatusCode = statusCode,
                Message = message,
                Details = exception.StackTrace // Esto podrías ocultarlo en producción
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}