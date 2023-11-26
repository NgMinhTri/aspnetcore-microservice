using Contracts.ScheduledJobs;
using Contracts.Services;
using Hangfire.API.Services;
using Hangfire.API.Services.Interfaces;
using Infrastructure.ScheduledJobs;
using Infrastructure.Services;
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

        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.AddTransient<IScheduledJobService, HangfireService>();
            services.AddScoped<IEmailSmtpService, EmailSmtpService>();
            services.AddTransient<IBackgroundJobService, BackgroundJobService>();
            return services;
        }
    }
}
