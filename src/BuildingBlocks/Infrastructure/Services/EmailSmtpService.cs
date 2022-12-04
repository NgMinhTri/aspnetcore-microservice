using Contracts.Services;
using Infrastructure.Configurations;
using MailKit.Net.Smtp;
using MimeKit;
using Serilog;
using Shared.Services.Email;

namespace Infrastructure.Services
{
    public class EmailSmtpService : IEmailSmtpService
    {
        private readonly ILogger _logger;
        private readonly EmailSMTPSetting _setting;
        private readonly SmtpClient _smtpClient;

        public EmailSmtpService(ILogger logger, EmailSMTPSetting settings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _setting = settings ?? throw new ArgumentNullException(nameof(settings));
            _smtpClient = new SmtpClient();
        }
        public async Task SendEmailAsync(MailRequest request, CancellationToken cancellationToken = default)
        {
            var emailMessage = new MimeMessage
            {
                Sender = new MailboxAddress(_setting.DisplayName, request.From ?? _setting.From),
                Subject = request.Subject,
                Body = new BodyBuilder
                {
                    HtmlBody = request.Body
                }.ToMessageBody()
            };

            if (request.ToAddresses.Any())
            {
                foreach(var toAddress in request.ToAddresses)
                {
                    emailMessage.To.Add(MailboxAddress.Parse(toAddress));
                }
            }
            else
            {
                var toAddress = request.ToAddress;
                emailMessage.To.Add(MailboxAddress.Parse(toAddress));
            }

            try
            {
                await _smtpClient.ConnectAsync(_setting.SMTPServer, _setting.Port, 
                    _setting.UseSsl, cancellationToken);
                await _smtpClient.AuthenticateAsync(_setting.Username, _setting.Password, cancellationToken);
                await _smtpClient.SendAsync(emailMessage, cancellationToken);
                await _smtpClient.DisconnectAsync(true, cancellationToken);

            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            finally
            {
                await _smtpClient.DisconnectAsync(true, cancellationToken);
                _smtpClient.Dispose();
            }
        }
    }
}
