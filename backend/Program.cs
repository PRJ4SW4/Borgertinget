using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.Hubs;
using backend.Repositories.Calendar;
using backend.Services;
using backend.Services.Calendar;
using backend.Services.Calendar.HtmlFetching;
using backend.Services.Calendar.Parsing;
using backend.Services.Calendar.Scraping;
using backend.Services.Flashcards;
using backend.Services.LearningEnvironment;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// for .env secrets
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

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
