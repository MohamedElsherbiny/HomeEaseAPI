using Massage.Application.Interfaces;
using Massage.Application.Interfaces.Repos;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Repositories;
using Massage.Infrastructure.Repos;
using Massage.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Massage.Domain.Entities;
using Massage.Infrastructure.Data;

namespace Massage.Application
{
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
}
