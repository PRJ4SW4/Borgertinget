using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.Hubs; // <--- TILFØJ DENNE LINJE
using backend.Services;
using backend.Services.AutomationServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore; // Inkluderer MigrateAsync()
using Microsoft.Extensions.Logging; // Inkluderer ILogger
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Enable detailed error messages for JWT validation (godt for udvikling)
IdentityModelEventSource.ShowPII = builder.Environment.IsDevelopment(); // Kun vis PII i udvikling
IdentityModelEventSource.LogCompleteSecurityArtifact = builder.Environment.IsDevelopment(); // Kun log fuld artifact i udvikling

// --- Konfiguration af Services ---

// JWT-konfiguration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(
    jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key mangler i konfigurationen")
);

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "RequireAdministratorRole",
        policy => policy.RequireRole(ClaimTypes.Role, "Admin") // Bruger standard role claim type
    );
    options.AddPolicy(
        "UserOrAdmin",
        policy => policy.RequireClaim(ClaimTypes.Role, "User", "Admin") // Bruger standard role claim type
    );
});

// EF Core Database Context
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."))
);

// Authentication med JWT Bearer
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // Kræv kun HTTPS i produktion
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
            ClockSkew = TimeSpan.Zero, // Vær præcis med token udløb
            RoleClaimType = ClaimTypes.Role, // Specificerer hvilket claim der indeholder roller
        };
        // Event hooks til debugging/logging af token validering
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Brug ILogger i stedet for Console.WriteLine i en rigtig applikation
                Console.WriteLine($"🚫 TOKEN VALIDATION FAILED: {context.Exception}");
                // Overvej at logge context.Exception for flere detaljer
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                 // Brug ILogger
                Console.WriteLine($"✅ TOKEN VALIDATED: User: {context.Principal?.Identity?.Name}, Roles: {string.Join(", ", context.Principal?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>())}");
                return Task.CompletedTask;
            },
             OnChallenge = context => {
                // Log når en udfordring sendes (f.eks. 401 Unauthorized)
                Console.WriteLine($"챌 CHALLENGE: {context.Error} - {context.ErrorDescription}");
                return Task.CompletedTask;
            },
            OnForbidden = context => {
                 // Log når adgang nægtes (f.eks. 403 Forbidden)
                 Console.WriteLine($"🚫 FORBIDDEN: User {context.Principal?.Identity?.Name} does not have required permissions.");
                 return Task.CompletedTask;
            }
        };
    });


builder.Services.AddSignalR();
// Swagger/OpenAPI konfiguration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "backendAPI", Version = "v1" });

    // Tilføj JWT authentication knap i Swagger UI
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

// Registrering af dine custom services
builder.Services.AddScoped<EmailService>();
builder.Services.AddHostedService<TweetFetchingService>(); // <--- TILFØJ DENNE LINJE
builder.Services.AddHttpClient<TwitterService>();

//oda.ft crawler
builder.Services.AddScoped<HttpService>();
builder.Services.AddScoped<IDailySelectionService, DailySelectionService>();

// CORS (Cross-Origin Resource Sharing) konfiguration
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowReactApp", // Navnet på din policy
        policy =>
        {
            policy.WithOrigins(builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    );
});

builder.Services.AddHttpClient(); // til OAuth

// Registrer Controllers
builder.Services.AddControllers();
builder.Services.AddScoped<IDailySelectionService, DailySelectionService>();
builder.Services.AddHostedService<DailySelectionJob>(); //* Bruges til udvælgelse af "dagens politiker"

// For altinget scraping
builder.Services.AddHttpClient();
builder.Services.AddScoped<AltingetScraperService>();
builder.Services.AddHostedService<ScheduledAltingetScrapeService>();

builder.Services.AddScoped<AdministratorService>();


// -----------------------------------------
// Byg WebApplication objektet
// -----------------------------------------
var app = builder.Build();

// --- DATABASE MIGRATION OG SEEDING START ---
// Dette afsnit køres én gang ved applikationens opstart
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>(); // Logger hentes her for scope't
    try
    {
        var context = services.GetRequiredService<DataContext>();

        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");


        logger.LogInformation("Attempting to seed data if necessary...");
        await PolidleSeed.SeedDataAsync(context, 75); // Kald til din seeder
        logger.LogInformation("Data seeding completed (data added only if tables were empty).");

    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        // throw; // Overvej at genkaste fejlen for at stoppe opstart
    }
}
// --- DATABASE MIGRATION OG SEEDING SLUT ---


// -----------------------------------------
// Konfigurer HTTP request pipeline (Middleware)
// Rækkefølgen er VIGTIG
// -----------------------------------------

// Konfigurer kun detaljeret fejlvisning og Swagger i udviklingsmiljøet
if (app.Environment.IsDevelopment())
{
    // Hent logger specifikt til denne blok
    var devLogger = app.Services.GetRequiredService<ILogger<Program>>();

    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();

    // Brug den hentede logger
    devLogger.LogInformation("Development environment detected. Swagger enabled.");
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// app.UseStaticFiles(); // Behold hvis du har filer i wwwroot

app.UseRouting();

app.UseCors("AllowReactApp"); // Skal typisk før UseAuthentication/UseAuthorization

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// For showing images in the frontend
app.UseStaticFiles();

app.MapHub<FeedHub>("/feedHub");
// Hent logger til den sidste besked før Run()
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("Starting application...");

app.Run(); // Starter applikationen
