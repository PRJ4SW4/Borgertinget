using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data; // Inkluderer DataContext og PolidleSeed
using backend.Services;
using backend.Services.AutomationServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore; // Inkluderer MigrateAsync()
using Microsoft.Extensions.Logging; // Inkluderer ILogger
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
    openSearchUrl = "http://localhost:9200"; 
    Console.WriteLine("Warning: OpenSearch URL not configured in appsettings.json. Using default: http://localhost:9200");
}

var settings = new ConnectionSettings(new Uri(openSearchUrl))
    .ServerCertificateValidationCallback(CertificateValidations.AllowAll) 
    .PrettyJson(); 

builder.Services.AddSingleton<IOpenSearchClient>(new OpenSearchClient(settings));

IdentityModelEventSource.ShowPII = true;
IdentityModelEventSource.LogCompleteSecurityArtifact = true;

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(
    jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key mangler i konfigurationen")
);

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

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."))
);

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); 
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
            ClockSkew = TimeSpan.Zero, 
            RoleClaimType = ClaimTypes.Role, 
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"🚫 TOKEN VALIDATION FAILED: {context.Exception.Message}");
                if (context.Exception is SecurityTokenExpiredException tokenExpiredException)
                {
                    Console.WriteLine($"Token expired at: {tokenExpiredException.Expires}");
                }
                else if (context.Exception is SecurityTokenInvalidSignatureException)
                {
                     Console.WriteLine($"Token signature invalid.");
                }
                // Add more specific logging if needed
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ TOKEN VALIDATED:");
                if (context.Principal?.Identity != null)
                {
                    Console.WriteLine("User: " + context.Principal.Identity?.Name);
                } else {
                    Console.WriteLine("User information not available after validation.");
                }
                return Task.CompletedTask;
            },
            OnForbidden = context => {
                 Console.WriteLine($"🚫 FORBIDDEN: User {context.Principal?.Identity?.Name} does not have required permissions for the resource.");
                 return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"챌린지 발생: {context.Error} - {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "backendAPI", Version = "v1" });
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

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<HttpService>();
builder.Services.AddHttpClient(); // Ensures IHttpClientFactory is available
builder.Services.AddScoped<IDailySelectionService, DailySelectionService>();
builder.Services.AddHostedService<DailySelectionJob>(); 
builder.Services.AddScoped<SearchIndexingService>();
builder.Services.AddScoped<AltingetScraperService>();
builder.Services.AddHostedService<ScheduledAltingetScrapeService>();
// builder.Services.AddHostedService<TestScheduledIndexService>(); // Keep this commented unless actively testing indexing

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReactApp", 
        policy =>
        {
            policy.WithOrigins(builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    );
});

builder.Services.AddControllers();

var app = builder.Build();

// --- OpenSearch Index Setup ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Ensuring OpenSearch index exists before starting...");
        await SearchIndexSetup.EnsureIndexExistsWithMapping(services);
        logger.LogInformation("OpenSearch Index check/creation complete.");

        // Consider if initial indexing should be run here or by the scheduled service.
        // If TestScheduledIndexService is enabled, it will run shortly after start.
        // If not, you might want to trigger an initial index if the index was just created:
        // var indexExistsResponse = await services.GetRequiredService<IOpenSearchClient>().Indices.ExistsAsync("borgertinget-search");
        // if (indexExistsResponse.Exists && !wasCreatedNow) { // you'd need a flag from EnsureIndexExists
        //    logger.LogInformation("Triggering initial background indexing task...");
        //    var indexingService = services.GetRequiredService<SearchIndexingService>();
        //    _ = indexingService.RunFullIndexAsync(); // Run in background, don't await
        // }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "An error occurred during application startup while setting up OpenSearch.");
    }
}

// --- DATABASE MIGRATION OG SEEDING START ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>(); 
    try
    {
        var context = services.GetRequiredService<DataContext>();

        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");

        logger.LogInformation("Attempting to seed Polidle-specific data (photos/quotes for existing Aktors)...");
        // Corrected call to the Polidle-specific seeder
        await PolidleSeed.SeedPolidleDataAsync(context); 
        logger.LogInformation("Polidle-specific data seeding completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}
// --- DATABASE MIGRATION OG SEEDING SLUT ---

if (app.Environment.IsDevelopment())
{
    var devLogger = app.Services.GetRequiredService<ILogger<Program>>();
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    devLogger.LogInformation("Development environment detected. Swagger enabled.");
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowReactApp"); 
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("Starting application...");

app.Run();
