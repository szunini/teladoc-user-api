using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using teladoc.domain.Exceptions;


namespace Teladoc.api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var traceId = context.TraceIdentifier;
                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;
            
            if (exception is UserNotFoundException notFound)
            {
                var pd = new ProblemDetails
                {
                    Title = "User not found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = notFound.Message,
                    Type = "https://httpstatuses.com/404",
                    Instance = context.Request.Path
                };
                pd.Extensions["traceId"] = context.TraceIdentifier;

                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(pd);
                return;
            }
            
            if (exception is UserValidationException validationEx)
            {
                var vpd = new ValidationProblemDetails(validationEx.Errors)
                {
                    Title = "Validation error",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = context.Request.Path
                };
                vpd.Extensions["traceId"] = context.TraceIdentifier;

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(vpd);
                return;
            }
            
            if (exception is ArgumentException argEx)
            {
                var vpd = new ValidationProblemDetails(new Dictionary<string, string[]>
                {             
                    [argEx.ParamName ?? "Request"] = new[] { argEx.Message }
                })
                {
                    Title = "Validation error",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = context.Request.Path
                };      
                vpd.Extensions["traceId"] = context.TraceIdentifier;

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(vpd);
                return;
            }
           
            if (TryGetUniqueViolation(exception, out var uniqueTarget))
            {
                var vpd = new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["Email"] = new[] { "Email already registered." }
                })
                {
                    Title = "Validation error",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = context.Request.Path
                };
                vpd.Extensions["traceId"] = context.TraceIdentifier;
                vpd.Extensions["constraint"] = uniqueTarget; 

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(vpd);
                return;
            }
            
            var pd500 = new ProblemDetails
            {
                Title = "Internal server error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred.",
                Instance = context.Request.Path
            };
            pd500.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(pd500);
        }
        private static bool TryGetUniqueViolation(Exception ex, out string? target)
        {
            target = null;
            
            var current = ex;
            while (current is not null)
            {
                if (current is DbUpdateException dbu)
                {
                    
                    if (dbu.InnerException?.Message?.Contains("IX_Users_EmailNormalized", StringComparison.OrdinalIgnoreCase) == true ||
                        dbu.InnerException?.Message?.Contains("IX_Users_Email", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        target = "Email unique index";
                        return true;
                    }

                    target = current.Message;
                    return true;
                }

                current = current.InnerException;
            }

            return false;
        }
    }
}