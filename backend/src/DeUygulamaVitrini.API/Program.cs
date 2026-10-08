using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using DeUygulamaVitrini.API.Middleware;
using DeUygulamaVitrini.Application;
using DeUygulamaVitrini.Infrastructure;
using DeUygulamaVitrini.Infrastructure.Persistence;
using DeUygulamaVitrini.Infrastructure.Persistence.Seeders;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Services ────────────────────────────────────────────────────────────────

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Global Exception Handler & ProblemDetails
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Katman Bağımlılık Kayıtları
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// Rate Limiter — Kötüye kullanım koruması (CAPTCHA endpoint için)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("CaptchaRateLimitPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 60,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// CORS — development ortamında yalnızca frontend dev sunucusuna izin ver.
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicies.DevelopmentFrontend, policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        if (allowedOrigins is { Length: > 0 })
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// OpenAPI / Swagger — development ortamında API keşfi için
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Demir Export Proje Kütüphanesi API",
        Version = "v1",
        Description = "Demir Export Ar-Ge Organizasyonu Proje Kütüphanesi REST API"
    });
});

// ─── Middleware Pipeline ──────────────────────────────────────────────────────

var app = builder.Build();

// Global Exception Handling middleware (en üst seviyede yer almalı)
app.UseExceptionHandler();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Demir Export API v1");
        options.RoutePrefix = "swagger";
    });

    if (app.Environment.IsEnvironment("Testing"))
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    // Development & Testing örnek veri tohumlaması (yalnızca dev/test ortamında)
    await DevelopmentDataSeeder.SeedAsync(app.Services);
}

// CORS middleware'i
app.UseCors(CorsPolicies.DevelopmentFrontend);

// Rate Limiter middleware'i
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.MapControllers();

app.Run();

// ─── Constants ───────────────────────────────────────────────────────────────

internal static class CorsPolicies
{
    public const string DevelopmentFrontend = "DevelopmentFrontend";
}
