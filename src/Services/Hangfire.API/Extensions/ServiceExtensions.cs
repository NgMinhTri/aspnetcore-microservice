using Shared.Configurations;

namespace Hangfire.API.Extensions
{
    public static class ServiceExtensions
    {
        internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var databaseSettings = configuration.GetSection(nameof(HangFireSettings))
                .Get<HangFireSettings>();
            services.AddSingleton(databaseSettings);
            return services;
        }
    }
}
