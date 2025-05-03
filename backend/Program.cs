using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.Services;
using backend.Services.AutomationServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenSearch.Client;
using OpenSearch.Net;

// for .env secrets
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

var openSearchUrl = builder.Configuration["OpenSearch:Url"];
if (string.IsNullOrEmpty(openSearchUrl))
{
    // Handle missing configuration - throw an error or default
    openSearchUrl = "http://localhost:9200"; // Default if not configured
    Console.WriteLine("Warning: OpenSearch URL not configured in appsettings.json. Using default: http://localhost:9200");
}

// Add credentials if needed (example using Basic Auth - get from config)
// var openSearchUser = builder.Configuration["OpenSearch:Username"];
// var openSearchPassword = builder.Configuration["OpenSearch:Password"];

// Configure the connection settings
var settings = new ConnectionSettings(new Uri(openSearchUrl))
    // Optional: Set a default index if most operations target one index
    // .DefaultIndex("your-default-index-name")
    // Optional: Add authentication if required
    // .BasicAuthentication(openSearchUser, openSearchPassword)
    // Optional: Disable SSL verification for local dev (NOT recommended for production)
    .ServerCertificateValidationCallback(CertificateValidations.AllowAll) // Use ONLY for dev/testing
    .PrettyJson(); // Makes debugging easier by formatting JSON requests/responses

// Register IOpenSearchClient as a singleton (recommended by the library)
builder.Services.AddSingleton<IOpenSearchClient>(new OpenSearchClient(settings));

// --- ADD OPENSEARCH CONFIGURATION AND REGISTRATION END ---


// Enable detailed error messages for JWT validation
IdentityModelEventSource.ShowPII = true;

// Enable detailed error messages for JWT validation
IdentityModelEventSource.ShowPII = true;
IdentityModelEventSource.LogCompleteSecurityArtifact = true;

// JWT-konfiguration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(
    jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key mangler")
);

// Authorization for Admin and User roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "RequireAdministratorRole",
        policy => policy.RequireRole(ClaimTypes.Role, "Admin")
    );
    options.AddPolicy(
        "UserOrAdmin",
        policy => policy.RequireClaim(ClaimTypes.Role, "User", "Admin")
    );
});

// EF Core
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Auth + JWT
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            // THIS LINE ensures ASP.NET picks up "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            // as the user's role claim.
            RoleClaimType = ClaimTypes.Role,
        };
        // 💥 Indsæt event hooks til fejllogning
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("🚫 TOKEN VALIDATION FAILED:");
                Console.WriteLine(context.Exception.ToString());
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ TOKEN VALIDATED:");
                if (context.Principal?.Identity != null)
                {
                    Console.WriteLine("User: " + context.Principal.Identity?.Name);
                    return Task.CompletedTask;
                } else {
                    Console.WriteLine("User information not available.");
                }
                return Task.CompletedTask;
            },
        };
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "backendAPI", Version = "v1" });

    // Tilføj JWT auth i Swagger
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Indtast JWT token i formatet: Bearer {token}",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

// Tilføj EmailService
builder.Services.AddScoped<EmailService>();

//oda.ft crawler
builder.Services.AddScoped<HttpService>();

builder.Services.AddHttpClient();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
        }
    );
});

builder.Services.AddHttpClient(); // til OAuth

builder.Services.AddControllers();
//Search indexing service
builder.Services.AddScoped<SearchIndexingService>();

// For altinget scraping
builder.Services.AddHttpClient();
builder.Services.AddScoped<AltingetScraperService>();
builder.Services.AddHostedService<ScheduledAltingetScrapeService>();
builder.Services.AddHostedService<TestScheduledIndexService>();


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>(); // Or specific category

    try
    {
        logger.LogInformation("Ensuring OpenSearch index exists before starting...");
        // Ensure SearchIndexSetup class exists or move this logic inline
        await SearchIndexSetup.EnsureIndexExistsWithMapping(services);
        logger.LogInformation("Index check/creation complete.");

        // --- Trigger initial indexing ---
        logger.LogInformation("Triggering initial background indexing task...");
        var indexingService = services.GetRequiredService<SearchIndexingService>();
        // --- End trigger initial indexing ---

    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "An error occurred during application startup while setting up OpenSearch.");
        // Optionally prevent the application from starting if setup fails
        // throw;
    }
}

// For static images from wwwroot folder
app.UseStaticFiles();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
