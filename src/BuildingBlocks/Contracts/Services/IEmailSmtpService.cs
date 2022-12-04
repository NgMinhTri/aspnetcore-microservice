using Shared.Services.Email;

namespace Contracts.Services
{
    public interface IEmailSmtpService : IEmailService<MailRequest>
    {

    }
}
