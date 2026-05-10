using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using WeatherAPI.Data;
using WeatherAPI.Middleware;
using WeatherAPI.Security;
using WeatherAPI.Services;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/weatherapi-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
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
            Description = "Secure Weather API with RBAC, Audit Logging, and Compliance Features"
        });
        
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme",
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
    var secretKey = builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured");
    
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
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    // Register custom services
    builder.Services.AddScoped<IWeatherService, WeatherService>();
    builder.Services.AddScoped<IAuditLogger, AuditLogger>();
    builder.Services.AddScoped<IEncryptionService, EncryptionService>();
    builder.Services.AddScoped<IRBACService, RBACService>();
    builder.Services.AddScoped<IDataRetentionService, DataRetentionService>();
    builder.Services.AddSingleton<ISecretsManager, AzureKeyVaultSecretsManager>();

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

    // Use custom middleware
    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseMiddleware<AuditLoggingMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();

    app.UseHttpsRedirection();
    app.UseCors("AllowSpecificOrigins");
    
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Starting Weather API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}