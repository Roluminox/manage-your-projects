using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MYP.Application.Common.Interfaces;
using MYP.Domain.Interfaces;
using MYP.Infrastructure.Identity;
using MYP.Infrastructure.Persistence;

namespace MYP.API.Tests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestSecret = "ThisIsAVerySecretKeyForTestingPurposesOnly123456!";
    public const string TestIssuer = "MYP";
    public const string TestAudience = "MYP";
    private readonly string _dbName = $"InMemoryTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove ALL EF Core related services
            var efServiceDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                    d.ServiceType == typeof(ApplicationDbContext) ||
                    d.ServiceType == typeof(IApplicationDbContext) ||
                    d.ServiceType == typeof(IUnitOfWork) ||
                    d.ServiceType.FullName?.Contains("EntityFramework") == true ||
                    d.ImplementationType?.FullName?.Contains("Npgsql") == true)
                .ToList();

            foreach (var descriptor in efServiceDescriptors)
            {
                services.Remove(descriptor);
            }

            // Create a separate service provider for EF Core InMemory
            var efServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseInMemoryDatabase(_dbName);
                options.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                options.UseInternalServiceProvider(efServiceProvider);
            });

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IUnitOfWork>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            // Configure JWT settings for testing (override the configuration)
            services.Configure<JwtSettings>(options =>
            {
                options.Secret = TestSecret;
                options.Issuer = TestIssuer;
                options.Audience = TestAudience;
                options.AccessTokenExpirationMinutes = 15;
                options.RefreshTokenExpirationDays = 7;
            });
        });
    }
}
