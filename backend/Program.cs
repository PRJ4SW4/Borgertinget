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
using backend.Enums; // Antager enums er her

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
    // Policy for Admin-rolle. Bruges f.eks. med [Authorize(Roles = "Admin")]
    // eller [Authorize(Policy = "RequireAdministratorRole")]
    // Sørg for at din JWT token indeholder en "role" claim med værdien "Admin"
    options.AddPolicy(
        "RequireAdministratorRole",
        policy => policy.RequireRole("Admin") // Direkte brug af RequireRole
    );
    // options.AddPolicy( // Alternativt med ClaimTypes.Role
    // "RequireAdministratorRole",
    //     policy => policy.RequireClaim(ClaimTypes.Role, "Admin")
    // );

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
            RoleClaimType = ClaimTypes.Role, // VIGTIGT for [Authorize(Roles = "...")]
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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Polidle API", Version = "v1" }); // Opdateret titel
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http, // Eller SecuritySchemeType.ApiKey hvis token sendes anderledes
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
builder.Services.AddScoped<HttpService>(); // Bruger du denne generisk, eller er den Twitter-specifik?
builder.Services.AddHostedService<TweetFetchingService>();
builder.Services.AddHttpClient<TwitterService>(); // HttpClientFactory for TwitterService
builder.Services.AddHttpClient(); // Generel HttpClientFactory

// For altinget scraping
builder.Services.AddScoped<AltingetScraperService>();
builder.Services.AddHostedService<ScheduledAltingetScrapeService>();


// *******************************************************************
// *** NYE DEPENDENCY INJECTION REGISTRERINGER FOR POLIDLE & UTILS ***
// *******************************************************************

// Utilities (typisk Singleton, da de er stateless)
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<IRandomProvider, RandomProvider>();

// Repositories (Scoped, da de bruger Scoped DbContext)
builder.Services.AddScoped<IAktorRepository, AktorRepository>();
builder.Services.AddScoped<IDailySelectionRepository, DailySelectionRepository>();
builder.Services.AddScoped<IGamemodeTrackerRepository, GamemodeTrackerRepository>();
// Tilføj andre repositories her hvis du laver flere (f.eks. IPartyRepository)

// Mappers og Algoritmer (Scoped er et sikkert valg, kan være Transient hvis ingen state)
builder.Services.AddScoped<IPoliticianMapper, PoliticianMapper>();
builder.Services.AddScoped<ISelectionAlgorithm, WeightedDateBasedSelectionAlgorithm>();

// Kerneservices (Scoped)
builder.Services.AddScoped<IDailySelectionService, DailySelectionService>();

// Baggrundsjobs (Hosted Services)
builder.Services.AddHostedService<DailySelectionJob>();

// Hvis du har DailySelectionJobSettings og vil injecte den via IOptions:
// builder.Services.Configure<DailySelectionJobSettings>(builder.Configuration.GetSection("DailySelectionJob"));

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
            policy.WithOrigins("http://localhost:5173") // Din frontend URL
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Vigtigt for SignalR med cookies/auth
        }
    );
});

builder.Services.AddControllers();
// Fjern .AddJsonOptions hvis du ikke har specifikke problemer med cirkulære referencer,
// da System.Text.Json som standard håndterer dette ok i .NET 6+ for simple tilfælde.
// Hvis du *har* brug for det:
// .AddJsonOptions(options =>
// {
//     options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles; // IgnoreCycles er ofte bedre end Preserve
// });


var app = builder.Build();

app.UseStaticFiles(); // For servering af f.eks. billeder fra wwwroot

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage(); // Giver mere detaljerede fejl i udvikling
}
else
{
    // Tilføj produktions-error handling her (f.eks. app.UseExceptionHandler("/Error"))
    app.UseHsts(); // Anbefales for produktion
}

// HTTPS Redirection - vigtigt for produktion
// app.UseHttpsRedirection(); // Aktiver denne hvis du har HTTPS sat op

app.UseCors("AllowReactApp");

app.UseRouting(); // Skal komme før Authentication og Authorization

app.UseAuthentication(); // VIGTIGT: Skal komme FØR UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.MapHub<FeedHub>("/feedHub"); // Sørg for at din Hub route matcher det, du tjekker i JWT Events

app.Run();