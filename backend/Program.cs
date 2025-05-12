using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims; // <<< TILFØJET FOR CLAIMS
using System.Text;
using backend.Data;
using backend.Services;
using backend.Services.AutomationServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using backend.Hubs;

// --- Nye using statements for DI ---
using backend.Interfaces.Repositories;
using backend.Persistence.Repositories;
using backend.Interfaces.Services;
using backend.Services.Selection;
using backend.Services.Mapping;
using backend.Interfaces.Utility;
using backend.Services.Utility;
using backend.Jobs;
using backend.Enums;

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
        policy => policy.RequireRole("Admin")
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
        options.RequireHttpsMetadata = false; // Sæt til true i produktion
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
            RoleClaimType = ClaimTypes.Role,
        };
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
                } else {
                    Console.WriteLine("User information not available.");
                }
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/feedHub")))
                {
                    Console.WriteLine("SIGNALR DEBUG: Setting token from query string.");
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Swagger
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Polidle API", Version = "v1" });
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

// --- EKSISTERENDE SERVICES ---
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<HttpService>();
builder.Services.AddHostedService<TweetFetchingService>();
builder.Services.AddHttpClient<TwitterService>();
builder.Services.AddHttpClient();

// For altinget scraping
builder.Services.AddScoped<AltingetScraperService>();
builder.Services.AddHostedService<ScheduledAltingetScrapeService>();

// *******************************************************************
// *** NYE DEPENDENCY INJECTION REGISTRERINGER FOR POLIDLE & UTILS ***
// *******************************************************************
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<IRandomProvider, RandomProvider>();
builder.Services.AddScoped<IAktorRepository, AktorRepository>();
builder.Services.AddScoped<IDailySelectionRepository, DailySelectionRepository>();
builder.Services.AddScoped<IGamemodeTrackerRepository, GamemodeTrackerRepository>();
builder.Services.AddScoped<IPoliticianMapper, PoliticianMapper>();
builder.Services.AddScoped<ISelectionAlgorithm, WeightedDateBasedSelectionAlgorithm>();
builder.Services.AddScoped<IDailySelectionService, DailySelectionService>();
builder.Services.AddHostedService<DailySelectionJob>();
// *******************************************************************
// *** SLUT PÅ NYE DI REGISTRERINGER                             ***
// *******************************************************************

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    );
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();

    // ***********************************************************************
    // *** START: Dummy Auth Middleware KUN FOR DEVELOPMENT ENVIRONMENT    ***
    // ***********************************************************************
    app.Use(async (context, next) =>
    {
        // Tjekker kun om der *ikke* allerede er en autentificeret bruger.
        // Hvis du ALTID vil være dummy-admin i dev, kan du fjerne denne ydre 'if'.
        // Dette tillader dig stadig at teste det rigtige login flow i dev, hvis du ønsker.
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "DummyDevAdmin"),       // Brugernavn for dummy admin
                new Claim(ClaimTypes.NameIdentifier, "dummy-admin-id-123"), // Et unikt ID
                new Claim(ClaimTypes.Role, "Admin"),              // Giver Admin rollen
                // Du kan tilføje flere claims her, f.eks. en email:
                // new Claim(ClaimTypes.Email, "devadmin@example.com")
            };
            var identity = new ClaimsIdentity(claims, "DevelopmentDummyAuth"); // "DevelopmentDummyAuth" er bare et navn
            context.User = new ClaimsPrincipal(identity);

            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>(); // Få en logger instans
            logger.LogWarning("DEVELOPMENT MODE: Bypassing JWT. User context populated with DummyDevAdmin (Role: Admin).");
        }
        await next.Invoke();
    });
    // ***********************************************************************
    // *** SLUT: Dummy Auth Middleware KUN FOR DEVELOPMENT ENVIRONMENT     ***
    // ***********************************************************************
}
else
{
    app.UseExceptionHandler("/Error"); // Sørg for at have en Error-handling side/endpoint
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Aktiver når du har HTTPS sat op

app.UseCors("AllowReactApp");

app.UseRouting(); // Skal være før Authentication og Authorization

app.UseAuthentication(); // Din rigtige JWT auth middleware
app.UseAuthorization();  // Din rigtige authorization middleware

app.MapControllers();
app.MapHub<FeedHub>("/feedHub");

app.Run();