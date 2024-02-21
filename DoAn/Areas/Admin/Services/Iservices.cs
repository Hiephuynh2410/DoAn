using Microsoft.AspNetCore.Identity;
using DoAn.Models;
using DoAn.Services;

namespace DoAn.Areas.Admin.Services
{

    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<ProductTypeServices>();
            return services;
        }
    }
}