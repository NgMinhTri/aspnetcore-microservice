using Contracts.ScheduledJobs;
using Contracts.Services;
using Hangfire.API.Services.Interfaces;
using Shared.Services.Email;
using ILogger = Serilog.ILogger;

namespace Hangfire.API.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IScheduledJobService _scheduledJobService;
        private readonly IEmailSmtpService _emailSmtpService;
        private readonly ILogger _logger;

        public BackgroundJobService(IScheduledJobService scheduledJobService,
                                    IEmailSmtpService emailSmtpService,
                                    ILogger logger)
        {
            _scheduledJobService = scheduledJobService;
            _emailSmtpService = emailSmtpService;
            _logger = logger;
        }

        public IScheduledJobService ScheduledJobService => _scheduledJobService;

        public string? SendEmailContent(string email, string subject, string emailContent, DateTimeOffset enqueueAt)
        {
            var emailRequest = new MailRequest
            {
                ToAddress = email,
                Body = emailContent,
                Subject = subject
            };

            try
            {
                var jobId = _scheduledJobService.Schedule(() =>  _emailSmtpService.SendEmail(emailRequest), enqueueAt);
                _logger.Information($"Sent email to {email} with subject: {subject} - Job Id: {jobId}");
                return jobId;
            }
            catch (Exception ex)
            {

                _logger.Error($"Failed due to an error with the email service: {ex.Message}");
            }

            return null;
        }
    }
}
