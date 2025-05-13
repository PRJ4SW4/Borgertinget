using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.Hubs;
using backend.Models;
using backend.Repositories.Calendar;
using backend.Services;
using backend.Services.Calendar;
using backend.Services.Calendar.HtmlFetching;
using backend.Services.Calendar.Parsing;
using backend.Services.Calendar.Scraping;
using backend.Services.Flashcards;
using backend.Services.LearningEnvironment;
using backend.Services.Search;
using backend.utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
    Console.WriteLine(
        "Warning: OpenSearch URL not configured in appsettings.json. Using default: http://localhost:9200"
    );
}

// Configure the connection settings
var settings = new ConnectionSettings(new Uri(openSearchUrl))
    .ServerCertificateValidationCallback(CertificateValidations.AllowAll) // ONLY for dev/testing
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

// EF Core
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder
    .Services.AddIdentity<User, IdentityRole<int>>(options =>
    {
        options.SignIn.RequireConfirmedEmail = true;

        options.Tokens.ProviderMap.Add(
            "CustomEmailConfirmation",
            new TokenProviderDescriptor(typeof(EmailConfirmationTokenProvider<User>))
        );
        options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";

        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<CostumErrorDescriber>()
    .AddRoleManager<RoleManager<IdentityRole<int>>>();

builder.Services.AddTransient<EmailConfirmationTokenProvider<User>>();

// Auth + JWT
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
        // Indsæt event hooks til fejllogning
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine(" TOKEN VALIDATION FAILED:");
                Console.WriteLine(context.Exception.ToString());
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine(" TOKEN VALIDATED:");
                if (context.Principal?.Identity != null)
                {
                    Console.WriteLine("User: " + context.Principal.Identity?.Name);
                    return Task.CompletedTask;
                }
                else
                {
                    Console.WriteLine("User information not available.");
                }
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                // Tjek om tokenet findes i 'access_token' query parameteren
                var accessToken = context.Request.Query["access_token"];

                // Tjek om requesten er til din SignalR Hub sti
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/feedHub"))) // <-- Match din Hub URL
                {
                    Console.WriteLine("SIGNALR DEBUG: Setting token from query string."); // <-- ADD LOG
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
        };
    });

// Authorization for Admin and User roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

// Swagger
builder.Services.AddSignalR();
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

builder.Services.AddHostedService<TweetFetchingService>();
builder.Services.AddHttpClient<TwitterService>();

builder.Services.AddHttpClient();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReactApp",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

builder.Services.AddHttpClient(); // General HttpClient factory registration (Used by OAuth and AltingetFetcher

// note af Jakob, put option id virkede med det her der er uddokumenteret, men da jeg havde det til, så virkede feed og partier ikke, jeg har ikke den post til pools på min git, derfor håber jeg det virker uden dette,


builder.Services.AddControllers(); /* .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });*/

//Search indexing service
builder.Services.AddScoped<SearchIndexingService>();

builder.Services.AddHttpContextAccessor(); // Gør IHttpContextAccessor tilgængelig
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>(); // Gør IActionContextAccessor tilgængelig

// For altinget scraping
builder.Services.AddHostedService<AltingetScraperServiceScheduler>();
builder.Services.AddScoped<IHtmlFetcher, AltingetHtmlFetcher>();
builder.Services.AddScoped<IEventDataParser, AltingetEventDataParser>();
builder.Services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();
builder.Services.AddScoped<IScraperService, AltingetScraperService>();
builder.Services.AddScoped<ICalendarService, CalendarService>();

// Learning Environment Services
builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<ILearningPageService, LearningPageService>();

// Flashcard Services
builder.Services.AddScoped<IFlashcardService, FlashcardService>();

builder.Services.AddRouting();

// Search Services
builder.Services.AddHostedService<ScheduledIndexService>();

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
        logger.LogCritical(
            ex,
            "An error occurred during application startup while setting up OpenSearch."
        );
        // Optionally prevent the application from starting if setup fails
        // throw;
    }
}

app.UseRouting();

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

app.MapHub<FeedHub>("/feedHub");
app.Run();
