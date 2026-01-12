using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartFactory.Application.Data;
using SmartFactory.Application.Helpers;
using SmartFactory.Api.Middleware;
using Serilog;
using System.Text;

// Configure Serilog
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "SmartFactory.Api")
    .CreateLogger();

try
{
    Log.Information("Starting SmartFactory API application");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Configure Swagger with JWT support
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartFactory API", Version = "v1" });

        // Add JWT Authentication
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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

    // Add JWT Authentication
    var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SmartFactory";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SmartFactoryUsers";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,  // Tắt để dễ test
            ValidateAudience = false, // Tắt để dễ test
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

    builder.Services.AddAuthorization();

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // Add Entity Framework
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Add Hangfire services
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

    // Add the processing server as IHostedService
    builder.Services.AddHangfireServer();

    // Register JwtHelper
    builder.Services.AddScoped<JwtHelper>();

    // Register ExcelImportService
    builder.Services.AddScoped<SmartFactory.Application.Services.ExcelImportService>();
    
    // Register WarehouseExcelService
    builder.Services.AddScoped<SmartFactory.Application.Services.WarehouseExcelService>();
    
    // Register StockInService
    builder.Services.AddScoped<SmartFactory.Application.Services.StockInService>();

    // Add MediatR
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Register Pipeline Behaviors
        cfg.AddOpenBehavior(typeof(SmartFactory.Application.Behaviors.UnhandledExceptionBehavior<,>));
        cfg.AddOpenBehavior(typeof(SmartFactory.Application.Behaviors.ValidationBehavior<,>));
        cfg.AddOpenBehavior(typeof(SmartFactory.Application.Behaviors.PerformanceBehavior<,>));
        cfg.AddOpenBehavior(typeof(SmartFactory.Application.Behaviors.LoggingBehavior<,>));
    });

    var app = builder.Build();

    // Seed Database
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var seeder = new SmartFactory.Application.Data.DbSeeder(context);
            await seeder.SeedAsync();
            Log.Information("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while seeding the database");
        }
    }

    // Configure the HTTP request pipeline
    
    // Enable global exception handling (must be first)
    app.UseGlobalExceptionHandler();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();

    // Enable CORS
    app.UseCors();

    // Enable Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Configure Hangfire Dashboard
    app.UseHangfireDashboard("/hangfire");

    app.MapControllers();

    Log.Information("SmartFactory API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("Shutting down SmartFactory API");
    Log.CloseAndFlush();
}

