using Backlogr.Api.Data;
using Backlogr.Api.Services.Implementations;
using Backlogr.Api.Services.Interfaces;
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
                ["Jwt:Audience"] = "Backlogr.Api.Tests.Client",
                ["Igdb:ClientId"] = "test-client-id",
                ["Igdb:ClientSecret"] = "test-client-secret",
                ["Igdb:ApiBaseUrl"] = "https://example.test",
                ["Cors:AllowedOrigins:0"] = "http://localhost",

                // Keep AI option validation happy in tests without using real external services.
                ["OpenAI:ApiKey"] = "test-openai-api-key",
                ["OpenAI:ChatModel"] = "gpt-test",
                ["OpenAI:EmbeddingModel"] = "text-embedding-test",
                ["AzureAiSearch:Endpoint"] = "https://example.search.windows.net",
                ["AzureAiSearch:ApiKey"] = "test-azure-search-api-key",
                ["AzureAiSearch:GamesIndexName"] = "games-test",

                // Prevent startup backfill from trying to run real indexing logic during tests.
                ["AiSearch:RunBackfillOnStartup"] = "false"
            };

            configBuilder.AddInMemoryCollection(testConfig);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            services.RemoveAll<IIgdbService>();

            services.RemoveAll<IRecommendationService>();
            services.RemoveAll<IReviewAssistantService>();
            services.RemoveAll<ISemanticSearchService>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            services.AddScoped<IIgdbService, StubIgdbService>();

            services.AddScoped<IRecommendationService, FakeRecommendationService>();
            services.AddScoped<IReviewAssistantService, FakeReviewAssistantService>();
            services.AddScoped<ISemanticSearchService, FakeSemanticSearchService>();
        });
    }
}