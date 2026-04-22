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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Por defecto es 500, pero podemos personalizarlo
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "Ocurrió un error inesperado en el servidor.";

            // Aquí capturamos tus excepciones personalizadas del Service
            if (exception is Exception && (exception.Message.Contains("registrado") || exception.Message.Contains("pertenece")))
            {
                statusCode = (int)HttpStatusCode.BadRequest; // 400
                message = exception.Message;
            }

            var response = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                // Solo mostrar detalles técnicos en desarrollo, no en producción
                Details = _env.IsDevelopment() ? exception.StackTrace : null
            };

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(response.ToString());
        }
    }
}