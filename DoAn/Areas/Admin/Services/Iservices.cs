﻿using Microsoft.AspNetCore.Identity;
using DoAn.Models;
using DoAn.Services;
using DoAn.ApiController.Services;
using DoAn.ApiController.Mail;

namespace DoAn.Areas.Admin.Services
{

    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            //Admin
            services.AddHttpContextAccessor();
            services.AddScoped<ProductTypeServices>();
            services.AddScoped<ProviderServices>();
            services.AddScoped<StaffServives>();
            services.AddScoped<ProductServices>();
            services.AddScoped<BranchServices>();
            services.AddScoped<LoginServices>();
            services.AddScoped<ServiceTypeServices>();
            services.AddScoped<GenerateRandomKey>();
            services.AddScoped<RoleServices>();
            services.AddScoped<BlogServices>();
            services.AddScoped<BlogCategoryServices>();




            //Client
            services.AddScoped<BookingServices>();
            services.AddScoped<SendMail>();
            services.AddScoped<ClientSearchProductServices>();
            services.AddScoped<BookingDateServices>();
            
            //general
            services.AddScoped<ForgotPassServices>();

            return services;
        }
    }
}