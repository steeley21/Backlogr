using System;
using System.Text;
using System.Text.Json.Serialization;
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
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserDeletionService, UserDeletionService>();
builder.Services.AddScoped<ISuperAdminBootstrapService, SuperAdminBootstrapService>();

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

builder.Services.AddScoped<IRecommendationService, StubRecommendationService>();
builder.Services.AddScoped<IReviewAssistantService, StubReviewAssistantService>();
builder.Services.AddScoped<ISemanticSearchService, StubSemanticSearchService>();

var app = builder.Build();

await IdentityDataSeeder.SeedRolesAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    await DevelopmentDataSeeder.SeedTestGameAsync(app.Services);
    await DevelopmentAdminSeeder.SeedAdminAsync(app.Services, app.Configuration);
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