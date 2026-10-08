using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Domain.Constants;
using DeUygulamaVitrini.Domain.Entities.Identity;
using DeUygulamaVitrini.Infrastructure.Persistence;
using DeUygulamaVitrini.Infrastructure.Persistence.Interceptors;
using DeUygulamaVitrini.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DeUygulamaVitrini.Infrastructure.Configuration;
using DeUygulamaVitrini.Infrastructure.Services.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using DeUygulamaVitrini.Application.Common.Models;
using DeUygulamaVitrini.Infrastructure.Services.Captcha;
using Microsoft.Extensions.Hosting;

namespace DeUygulamaVitrini.Infrastructure;

/// <summary>
/// Infrastructure katmanının dependency injection kayıtlarını
/// merkezi olarak yönetir.
/// API katmanının Program.cs dosyasından çağrılır.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        // ─── Connection String & Test Isolation Safety Guard ──────────────────
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        if (environment != null && environment.IsEnvironment("Testing"))
        {
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            var initialCatalog = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(initialCatalog) ||
                initialCatalog.Equals("DeUygulamaVitriniDb", StringComparison.OrdinalIgnoreCase) ||
                !initialCatalog.EndsWith("TestDb", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"TEST DATABASE ISOLATION SAFETY GUARD VIOLATION: Environment is 'Testing' but connection string targets database '{initialCatalog}'. " +
                    $"Testing environment MUST target a dedicated test database (e.g., 'DeUygulamaVitriniTestDb'). Startup aborted to protect Development database.");
            }
        }

        // ─── Interceptors ─────────────────────────────────────────────────────
        services.AddSingleton<AuditableEntityInterceptor>();

        // ─── EF Core — SQL Server ─────────────────────────────────────────────
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServerOptions =>
                {
                    sqlServerOptions.MigrationsAssembly(
                        typeof(ApplicationDbContext).Assembly.FullName);
                    sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                }));

        // ─── IApplicationDbContext → ApplicationDbContext ─────────────────────
        services.AddScoped<IApplicationDbContext>(
            provider => provider.GetRequiredService<ApplicationDbContext>());

        // ─── ASP.NET Core Identity ────────────────────────────────────────────
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // ─── Cookie Authentication Configuration ──────────────────────────────
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = ".DemirExport.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        // ─── Authorization Policies ───────────────────────────────────────────
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AuthenticatedUser", policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy("SuperAdminAccess", policy =>
                policy.RequireRole(AppRoles.SuperAdmin));

            options.AddPolicy("AdminAccess", policy =>
                policy.RequireRole(AppRoles.SuperAdmin, AppRoles.Admin));

            options.AddPolicy("OrganizationManagementAccess", policy =>
                policy.RequireRole(AppRoles.SuperAdmin, AppRoles.Admin));
        });

        // ─── Authorization & File & Audit & Notification & Excel Services ────
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();
        services.AddScoped<IModuleAccessService, ModuleAccessService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<IProjectImportFileStore, MemoryProjectImportFileStore>();
        services.AddScoped<IProjectExcelService, ProjectExcelService>();

        // ─── First-Party CAPTCHA Challenge Security Layer ─────────────────────
        services.Configure<CaptchaOptions>(configuration.GetSection(CaptchaOptions.SectionName));
        services.AddSingleton<ICaptchaChallengeStore, MemoryCaptchaChallengeStore>();
        services.AddScoped<ICaptchaService, CaptchaService>();

        // ─── AI Provider Configuration & Typed HttpClients ───────────────────
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.AddHttpClient<IAiProvider, LocalAiProvider>((serviceProvider, client) =>
        {
            var options = configuration.GetSection(AiOptions.SectionName).Get<AiOptions>() ?? new AiOptions();
            var timeoutSeconds = options.RequestTimeoutSeconds > 0 ? options.RequestTimeoutSeconds : 60;
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

        services.AddHttpClient<IEmbeddingProvider, LocalEmbeddingProvider>((serviceProvider, client) =>
        {
            var options = configuration.GetSection(AiOptions.SectionName).Get<AiOptions>() ?? new AiOptions();
            var timeoutSeconds = options.RequestTimeoutSeconds > 0 ? options.RequestTimeoutSeconds : 60;
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

        // ─── AI Knowledge Index, Semantic Search & RAG Assistant Services ────
        services.AddScoped<IProjectKnowledgeDocumentBuilder, ProjectKnowledgeDocumentBuilder>();
        services.AddScoped<IProjectKnowledgeIndexService, ProjectKnowledgeIndexService>();
        services.AddScoped<IProjectSemanticSearchService, ProjectSemanticSearchService>();
        services.AddScoped<IProjectAssistantService, ProjectAssistantService>();
        services.AddScoped<IProjectAiSummaryService, ProjectAiSummaryService>();
        services.AddScoped<IAiWarmupService, AiWarmupService>();

        // ─── Automatic Semantic Index Background Reconciliation Worker ───────
        services.AddHostedService<SemanticIndexBackgroundWorker>();

        // ─── Non-blocking Provider-Agnostic AI Cold-Start Warmup Worker ───────
        services.AddHostedService<AiWarmupBackgroundService>();

        return services;
    }
}
