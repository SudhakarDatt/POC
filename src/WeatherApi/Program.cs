using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using WeatherApi.Data;
using WeatherApi.Middleware;
using WeatherApi.Services;
using WeatherApi.Security;
using FluentValidation;
using FluentValidation.AspNetCore;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/weather-api-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting Weather API application");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Configure Swagger with JWT support
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Weather API",
            Version = "v1",
            Description = "Secure Weather API with RBAC, Audit Logging, and Compliance Features",
            Contact = new OpenApiContact
            {
                Name = "API Support",
                Email = "support@weatherapi.com"
            }
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // Configure Database
    builder.Services.AddDbContext<WeatherDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Configure JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
    });

    // Register application services
    builder.Services.AddScoped<IWeatherService, WeatherService>();
    builder.Services.AddScoped<IAuditLogger, AuditLogger>();
    builder.Services.AddScoped<IEncryptionService, EncryptionService>();
    builder.Services.AddScoped<IRBACService, RBACService>();
    builder.Services.AddScoped<IDataRetentionService, DataRetentionService>();
    builder.Services.AddScoped<IConsentManagementService, ConsentManagementService>();
    builder.Services.AddSingleton<ISecretsManager, SecretsManager>();

    // Add FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddFluentValidationAutoValidation();

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins",
            policy =>
            {
                policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
    });

    // Add Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<WeatherDbContext>();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Security Headers Middleware
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
        await next();
    });

    app.UseHttpsRedirection();
    app.UseCors("AllowSpecificOrigins");

    // Custom Middleware
    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseMiddleware<AuditLoggingMiddleware>();
    app.UseMiddleware<RateLimitingMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}