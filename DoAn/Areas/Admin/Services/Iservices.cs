using Microsoft.AspNetCore.Identity;
using DoAn.Models;
using DoAn.Services;

namespace DoAn.Areas.Admin.Services
{

    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddScoped<ProductTypeServices>();
            services.AddScoped<ProviderServices>();
            services.AddScoped<StaffServives>();
            services.AddScoped<ProductServices>();
            services.AddScoped<BranchServices>();
            services.AddScoped<LoginServices>();
            services.AddScoped<GenerateRandomKey>();

            return services;
        }
    }
}