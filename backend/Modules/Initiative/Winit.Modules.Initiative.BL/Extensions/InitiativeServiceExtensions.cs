using Microsoft.Extensions.DependencyInjection;
using Winit.Modules.Initiative.BL.Classes;
using Winit.Modules.Initiative.BL.Interfaces;
using Winit.Modules.Initiative.DL.Classes;
using Winit.Modules.Initiative.DL.Interfaces;

namespace Winit.Modules.Initiative.BL.Extensions
{
    public static class InitiativeServiceExtensions
    {
        public static IServiceCollection AddInitiativeModule(this IServiceCollection services)
        {
            services.AddScoped<IInitiativeDL, PGSQLInitiativeDL>();
            
            services.AddScoped<IInitiativeBL, InitiativeBL>();
            
            services.AddAutoMapper(typeof(InitiativeBL).Assembly);
            
            return services;
        }
    }
}