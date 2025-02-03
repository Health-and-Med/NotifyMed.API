using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace NotifyMed.Application.Services
{
    public class EmailService
    {
        private readonly string _sendGridApiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _sendGridApiKey = configuration["SendGrid:ApiKey"];
            _fromEmail = configuration["SendGrid:FromEmail"];
            _fromName = configuration["SendGrid:FromName"];
        }

        public async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string messageBody)
        {
            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail, toName);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, messageBody, messageBody);

            var response = await client.SendEmailAsync(msg);
            var r = response.StatusCode;
            return response.StatusCode == System.Net.HttpStatusCode.Accepted;
        }
    }
}
