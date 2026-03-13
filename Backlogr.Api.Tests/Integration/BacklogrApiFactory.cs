using Backlogr.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backlogr.Api.Tests.Integration;

public class BacklogrApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"BacklogrApiTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var testConfig = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=BacklogrTestDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True",
                ["Jwt:Key"] = "Backlogr-Test-Jwt-Key-This-Is-Long-Enough-For-HS256-12345",
                ["Jwt:Issuer"] = "Backlogr.Api.Tests",
                ["Jwt:Audience"] = "Backlogr.Api.Tests.Client"
            };

            configBuilder.AddInMemoryCollection(testConfig);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });
    }
}