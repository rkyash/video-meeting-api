using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using VideoMeeting.Api.Endpoints;
using VideoMeeting.Api.Extensions;
using VideoMeeting.Api.Middleware;
using VideoMeeting.Application;
using VideoMeeting.Application.Common.Models;
using VideoMeeting.Infrastructure;
using VideoMeeting.Infrastructure.Persistence;
using VideoMeeting.Shared.Configuration;
using VideoMeeting.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Add configuration classes
builder.Services.AddAppConfiguration(builder.Configuration);

// Add layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// JWT Authentication Configuration using configuration class
var jwtConfig = builder.Configuration.GetSection(JwtConfiguration.SectionName).Get<JwtConfiguration>()
                ?? throw new InvalidOperationException("JWT configuration is missing");
var key = Encoding.ASCII.GetBytes(jwtConfig.Key);

builder.Services.AddAuthentication(options =>
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
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtConfig.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// SignalR Configuration
builder.Services.AddSignalR();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// OpenAPI Configuration for Scalar UI
builder.Services.AddOpenApi();

var app = builder.Build();

// Ensure database is created
// Replace EnsureCreated() with migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    if (app.Environment.IsDevelopment())
    {
        await context.Database.MigrateAsync(); // Auto-apply in development
    }
    else
    {
        // Production: Check for pending migrations
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Pending migrations: {Migrations}", 
                string.Join(", ", pendingMigrations));
            
            // Uncomment to auto-apply in production (use with caution)
            await context.Database.MigrateAsync();
        }
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Add Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "Handled {RequestPath} ({RequestMethod}) in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? LogEventLevel.Error
        : httpContext.Response.StatusCode > 499
            ? LogEventLevel.Error
            : LogEventLevel.Information;
});

app.UseCors("AllowAll");

// Add custom response middleware
app.UseMiddleware<ResponseMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Map Minimal API Endpoints
app.MapAuthEndpoints();
app.MapMeetingEndpoints();

// System Endpoints
app.MapGet("/health", () => new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Architecture = "Clean Architecture with Minimal APIs",
        UI = "Scalar API Documentation",
        Layers = new[] { "Domain", "Application", "Infrastructure", "Minimal APIs" }
    }.ToApiResponse("Health check completed successfully"))
    .WithName("HealthCheck")
    .WithTags("System")
    .WithSummary("Health check endpoint")
    .WithDescription("Returns the current health status of the API")
    .Produces<ApiResponse<object>>()
    .WithOpenApi();

app.MapGet("/api/info", () => new
    {
        ApplicationName = "Video Meeting API - Clean Architecture with Minimal APIs",
        Version = "1.0.0",
        Environment = app.Environment.EnvironmentName,
        Framework = ".NET 9",
        Architecture = "Clean Architecture",
        APIStyle = "Minimal APIs",
        Documentation = "Scalar UI",
        VideoSDK = "Vonage Video API",
        Database = "PostgreSQL",
        Features = new[]
        {
            "Clean Architecture with Domain-Driven Design",
            "Minimal APIs with OpenAPI",
            "CQRS with MediatR",
            "JWT Authentication",
            "Video Meetings with Vonage",
            "Screen Sharing",
            "Meeting Recording",
            "Guest Participants",
            "Real-time SignalR Communication",
            "Scalar API Documentation",
            "Entity Framework Core with PostgreSQL"
        }
    }.ToApiResponse("API information retrieved successfully"))
    .WithName("GetApiInfo")
    .WithTags("System")
    .WithSummary("API information endpoint")
    .WithDescription("Returns detailed information about the API and its features")
    .Produces<ApiResponse<object>>()
    .WithOpenApi();

try
{
    Log.Information("Starting Video Meeting API");
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