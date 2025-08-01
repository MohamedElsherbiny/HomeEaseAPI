using HomeEase.Application.Interfaces;
using HomeEase.Application.Interfaces.Repos;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Repositories;
using HomeEase.Infrastructure.Repos;
using HomeEase.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using HomeEase.Domain.Entities;
using HomeEase.Infrastructure.Data;
using HomeEase.Infrastructure.Services.Export;

namespace HomeEase.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ✅ Corrected DbContext registration
        services.AddDbContext<IAppDbContext, AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.UseNetTopologySuite()));

        // ✅ Repositories
        services.AddScoped<IProviderRepository, ProviderRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ✅ Domain/Infra services
        services.AddScoped<INotificationService, NotificationService>();

        // ✅ Configuration bindings
        services.Configure<NotificationSettings>(configuration.GetSection("NotificationSettings"));
        services.AddScoped<IDataExportService, DataExportService>();

        services.AddTransient<ExportExcelHtmlAgilityPackClosedXMLService>();
        services.AddTransient<ExportExcelAngleSharpClosedXMLService>();
        services.AddTransient<Func<EnumExportExcelType, IExportExcelService>>(provider => key =>
        {
            return key switch
            {
                EnumExportExcelType.HtmlAgilityPack => provider.GetRequiredService<ExportExcelHtmlAgilityPackClosedXMLService>(),
                EnumExportExcelType.AngleSharpClosedXML => provider.GetRequiredService<ExportExcelAngleSharpClosedXMLService>(),
                _ => throw new ArgumentException($"Unknown key: {key}")
            };
        });

        return services;
    }
}
