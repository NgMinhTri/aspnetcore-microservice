using Shared.Configurations;
using System.Text;

namespace Basket.API.Services
{
    public class EmailTemplateService
    {
        protected readonly BackgroundJobSettings _backgroundJobSettings;
        public EmailTemplateService(BackgroundJobSettings backgroundJobSettings)
        {
            _backgroundJobSettings = backgroundJobSettings;
        }

        private static readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string _tmplFolder = Path.Combine(_baseDirectory, "EmailTemplates");

        protected string ReadEmailTemplateContent(string templateEmailName, string format = "html")
        {
            var filePath = Path.Combine(_tmplFolder, templateEmailName + "." + format);
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs, Encoding.Default);
            var emailText = sr.ReadToEnd();
            sr.Close();
            return emailText;
        }
    }
}
