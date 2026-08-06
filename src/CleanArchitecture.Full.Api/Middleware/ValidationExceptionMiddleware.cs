using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FluentValidation;

namespace CleanArchitecture.Full.Api.Middleware;

public static class ValidationExceptionMiddlewareExtensions
{
    private const long MaximumLoggedBodySize = 16 * 1024;

    public static IApplicationBuilder UseValidationExceptionHandling(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("CleanArchitecture.Full.Api.Middleware.ExceptionHandling");

            var stopwatch = Stopwatch.StartNew();

            var routeValues = context.Request.RouteValues
                .ToDictionary(item => item.Key, item => item.Value?.ToString());

            var queryParameters = context.Request.Query
                .ToDictionary(item => item.Key, item => item.Value.ToArray());

            var requestBody = await ReadRequestBodyAsync(context.Request, context.RequestAborted);

            logger.LogInformation(
                "Iniciando HTTP {Method} {Path}. Route: {@RouteValues}, Query: {@QueryParameters}, Body: {@RequestBody}",
                context.Request.Method,
                context.Request.Path,
                routeValues,
                queryParameters,
                requestBody);

            try
            {
                await next(context);
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                // Una regla de negocio/entrada inválida no es un error del servidor:
                // se registra como Warning, no como Error.
                logger.LogWarning(
                    "Validación fallida por los siguientes motivos: {ErrorCount} error(es) -> {@Errors}",
                    errors.Count,
                    errors);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Validation error",
                    status = StatusCodes.Status400BadRequest,
                    errors
                });
            }
            catch (Exception ex)
            {
                // Excepción no controlada -> Error, con la excepción completa para el stack trace.
                logger.LogError(
                    ex,
                    "Excepción no controlada del siguiente endpoint {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "An unexpected error occurred",
                    status = StatusCodes.Status500InternalServerError
                });
            }
            finally
            {
                stopwatch.Stop();

                var completionLevel = context.Response.StatusCode switch
                {
                    >= StatusCodes.Status500InternalServerError => LogLevel.Error,
                    >= StatusCodes.Status400BadRequest => LogLevel.Warning,
                    _ => LogLevel.Information
                };

                logger.Log(
                    completionLevel,
                    "HTTP {Method} {Path} respondió {StatusCode} en {ElapsedMilliseconds} ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        });

    private static async Task<object?> ReadRequestBodyAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ContentLength is null or 0 ||
            request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) != true)
        {
            return null;
        }

        if (request.ContentLength > MaximumLoggedBodySize)
        {
            return new { Omitted = true, Reason = "Body demasiado grande", request.ContentLength };
        }

        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync(cancellationToken);
        request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return ConvertJsonElement(document.RootElement);
        }
        catch (JsonException)
        {
            return new { InvalidJson = true };
        }
    }

    private static object? ConvertJsonElement(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(property => property.Name, property => ConvertJsonElement(property.Value)),
            JsonValueKind.Array => element.EnumerateArray()
                .Select(ConvertJsonElement)
                .ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt64(out var integer) => integer,
            JsonValueKind.Number when element.TryGetDecimal(out var number) => number,
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => element.GetRawText()
        };
}
