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
        services.AddScoped<IPaymentProcessor, PaymentProcessor>();

        // ✅ Configuration bindings
        services.Configure<NotificationSettings>(configuration.GetSection("NotificationSettings"));
        services.Configure<PaymentSettings>(configuration.GetSection("PaymentSettings"));

        return services;
    }
}
