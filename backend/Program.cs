using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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
                Console.WriteLine("User: " + context.Principal.Identity?.Name);
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
builder.Services.AddHostedService<TweetFetchingService>(); // <--- TILFØJ DENNE LINJE

builder.Services.AddHttpClient<TwitterService>();


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

builder.Services.AddControllers();

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
app.Run();
