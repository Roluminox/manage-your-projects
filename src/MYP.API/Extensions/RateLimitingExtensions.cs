using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace MYP.API.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthRateLimitPolicy = "AuthRateLimit";
    public const int DefaultPermitLimit = 5;
    public const int DefaultWindowMinutes = 15;

    public static IServiceCollection AddRateLimitingServices(
        this IServiceCollection services,
        int? permitLimit = null,
        int? windowMinutes = null)
    {
        var limit = permitLimit ?? DefaultPermitLimit;
        var window = windowMinutes ?? DefaultWindowMinutes;

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(AuthRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIdentifier(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limit,
                        Window = TimeSpan.FromMinutes(window),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    title = "Too Many Requests",
                    message = "Rate limit exceeded. Please try again later.",
                    retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                        ? retryAfter.TotalSeconds
                        : window * 60
                }, cancellationToken);
            };
        });

        return services;
    }

    public static IApplicationBuilder UseRateLimitingMiddleware(this IApplicationBuilder app)
    {
        return app.UseRateLimiter();
    }

    private static string GetClientIdentifier(HttpContext httpContext)
    {
        // Use X-Forwarded-For header if behind a proxy, otherwise use remote IP
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP in the chain (original client)
            return forwardedFor.Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
