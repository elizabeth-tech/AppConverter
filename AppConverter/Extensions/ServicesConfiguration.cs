using AppConverter.Services;
using AppConverter.Services.Interfaces;

namespace AppConverter.API.Extensions
{
    public static class ServicesConfiguration
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IConverterService, ConverterService>();
            return services;
        }
    }
}
