using DeUygulamaVitrini.Application.Common.Interfaces;
using DeUygulamaVitrini.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DeUygulamaVitrini.Application;

/// <summary>
/// Application katmanının bağımlılık kaydı.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<ITeamsService, TeamsService>();
        services.AddScoped<IAdminOrganizationService, AdminOrganizationService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IProfileService, ProfileService>();

        return services;
    }
}
