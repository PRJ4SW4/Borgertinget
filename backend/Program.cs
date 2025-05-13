using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims; // <<< TILFØJET FOR CLAIMS
using System.Text;
using backend.Data;
using backend.Hubs;
using backend.Services;
using backend.Models;
// For Altinget Scraping
using backend.Services.AutomationServices;
using backend.Services.AutomationServices.HtmlFetching;
using backend.Services.AutomationServices.Parsing;
using backend.Services.AutomationServices.Repositories;
// Flashcard Services
using backend.Services.Flashcards;
// Learning Environment Services
using backend.Services.LearningEnvironmentServices;
// JWT Stuff
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using backend.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using backend.utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options; // Add this using directive
using Microsoft.AspNetCore.DataProtection;  // Add this
using Microsoft.Extensions.Logging; // Add this


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

builder.Services
    .AddIdentity<User, IdentityRole<int>>(options => {
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
            // THIS LINE ensures ASP.NET picks up "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            // as the user's role claim.
            RoleClaimType = ClaimTypes.Role
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
    options.AddPolicy(
        "RequireAdministratorRole",
        policy => policy.RequireRole("Admin")
    );
    options.AddPolicy(
        "UserOrAdmin",
        policy => policy.RequireRole("User", "Admin")
    );
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

builder.Services.AddHttpContextAccessor(); // Gør IHttpContextAccessor tilgængelig
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>(); // Gør IActionContextAccessor tilgængelig

// For altinget scraping
builder.Services.AddHostedService<ScheduledAltingetScrapeService>();
builder.Services.AddScoped<IHtmlFetcher, AltingetHtmlFetcher>();
builder.Services.AddScoped<IEventDataParser, AltingetEventDataParser>();
builder.Services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();
builder.Services.AddScoped<IAutomationService, AltingetScraperService>();

// Learning Environment Services
builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<ILearningPageService, LearningPageService>();

// Flashcard Services
builder.Services.AddScoped<IFlashcardService, FlashcardService>();

builder.Services.AddRouting();

var app = builder.Build();

app.UseRouting();
// For static images from wwwroot folder
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
        // ALTID i development, sæt dummy admin:
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "DummyDevAdmin"),       // Brugernavn for dummy admin
                new Claim(ClaimTypes.NameIdentifier, "dummy-admin-id-123"), // Et unikt ID
                new Claim(ClaimTypes.Role, "Admin"),              // Giver Admin rollen
                new Claim(ClaimTypes.Email, "devadmin@example.com"),
            };
            var identity = new ClaimsIdentity(claims, "DevelopmentDummyAuth"); // "DevelopmentDummyAuth" er bare et navn
            context.User = new ClaimsPrincipal(identity);

            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>(); // Få en logger instans
            logger.LogWarning("DEVELOPMENT MODE: Bypassing JWT. User context populated with DummyDevAdmin (Role: Admin).");

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