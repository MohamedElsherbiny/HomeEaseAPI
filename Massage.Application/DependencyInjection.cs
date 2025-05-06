using Microsoft.Extensions.DependencyInjection;

using System.Reflection;
using MediatR;


namespace Massage.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register MediatR (Application layer)
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register AutoMapper (Application layer)
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddAutoMapper(typeof(Massage.Application.Mappings.BookingMappingProfile).Assembly);
            return services;
        }
    }
}

