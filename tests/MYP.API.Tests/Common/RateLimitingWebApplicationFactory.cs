using Microsoft.Extensions.DependencyInjection;

namespace MYP.API.Tests.Common;

public class RateLimitingWebApplicationFactory : CustomWebApplicationFactory
{
    protected override void DisableRateLimiting(IServiceCollection services)
    {
        // Do not disable rate limiting - keep the default configuration
        // This allows rate limiting tests to verify the behavior
    }
}
