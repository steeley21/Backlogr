using Backlogr.Api.Data;
using Backlogr.Api.Extensions;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Options;
using Backlogr.Api.Services.Implementations;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration section was not found.");

if (string.IsNullOrWhiteSpace(jwtOptions.Key) ||
    string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
    string.IsNullOrWhiteSpace(jwtOptions.Audience))
{
    throw new InvalidOperationException("JWT configuration is incomplete.");
}

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.Configure<BootstrapSuperAdminOptions>(
    builder.Configuration.GetSection(BootstrapSuperAdminOptions.SectionName));

builder.Services.AddOptions<IgdbOptions>()
    .Bind(builder.Configuration.GetSection(IgdbOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ClientId),
        "Igdb:ClientId is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ClientSecret),
        "Igdb:ClientSecret is required.")
    .ValidateOnStart();

builder.Services.AddOptions<OpenAiOptions>()
    .Bind(builder.Configuration.GetSection(OpenAiOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ApiKey),
        "OpenAI:ApiKey is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ChatModel),
        "OpenAI:ChatModel is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.EmbeddingModel),
        "OpenAI:EmbeddingModel is required.")
    .ValidateOnStart();

builder.Services.AddOptions<AzureAiSearchOptions>()
    .Bind(builder.Configuration.GetSection(AzureAiSearchOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Endpoint),
        "AzureAiSearch:Endpoint is required.")
    .Validate(
        options => Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _),
        "AzureAiSearch:Endpoint must be a valid absolute URI.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ApiKey),
        "AzureAiSearch:ApiKey is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.GamesIndexName),
        "AzureAiSearch:GamesIndexName is required.")
    .ValidateOnStart();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.UseInlineDefinitionsForEnums();
    options.SchemaFilter<EnumDescriptionSchemaFilter>();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Backlogr.Api",
        Version = "v1",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer",
    });

    options.AddSecurityRequirement(document => new()
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReviewInteractionService, ReviewInteractionService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserDeletionService, UserDeletionService>();
builder.Services.AddScoped<ISuperAdminBootstrapService, SuperAdminBootstrapService>();
builder.Services.AddScoped<IAiSearchSyncService, AiSearchSyncService>();

builder.Services.AddSingleton<IEmbeddingService, OpenAiEmbeddingService>();
builder.Services.AddSingleton<IAiSearchIndexService, AzureAiSearchIndexService>();

builder.Services.AddHttpClient<ITwitchTokenService, TwitchTokenService>(httpClient =>
{
    httpClient.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddHttpClient<IIgdbService, IgdbService>((serviceProvider, httpClient) =>
{
    var igdbOptions = serviceProvider.GetRequiredService<IOptions<IgdbOptions>>().Value;

    httpClient.BaseAddress = new Uri(igdbOptions.ApiBaseUrl);
    httpClient.Timeout = TimeSpan.FromSeconds(20);
});

builder.Services.AddScoped<IRecommendationService, AzureRecommendationService>();
builder.Services.AddScoped<ISemanticSearchService, AzureSemanticSearchService>();

builder.Services.AddHttpClient<IReviewAssistantService, OpenAiReviewAssistantService>(
    (serviceProvider, httpClient) =>
    {
        var openAiOptions = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;

        httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", openAiOptions.ApiKey);
        httpClient.Timeout = TimeSpan.FromSeconds(30);
    });

var app = builder.Build();

await IdentityDataSeeder.SeedRolesAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    await DevelopmentDataSeeder.SeedTestGameAsync(app.Services);
    await DevelopmentAdminSeeder.SeedAdminAsync(app.Services, app.Configuration);
}

using (var scope = app.Services.CreateScope())
{
    var aiSearchSyncService = scope.ServiceProvider.GetRequiredService<IAiSearchSyncService>();
    await aiSearchSyncService.BackfillGamesAsync();
}

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}