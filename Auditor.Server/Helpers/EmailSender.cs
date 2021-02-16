using System;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Auditor.Server.Helpers
{
    public class EmailSender : IEmailSender
    {
        private readonly MailJetSettings _mailJetSettings;

        public EmailSender(IOptions<MailJetSettings> mailJetSettings)
        {
            _mailJetSettings = mailJetSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            MailjetClient client =
                new MailjetClient(_mailJetSettings.PublicKey, _mailJetSettings.PrivateKey);
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            };

            // construct your email with builder
            var malJetEmail = new TransactionalEmailBuilder()
                .WithFrom(new SendContact(_mailJetSettings.Email))
                .WithSubject(subject)
                .WithHtmlPart(htmlMessage)
                .WithTo(new SendContact(email))
                .Build();

            // invoke API to send malJetEmail
            var response = await client.SendTransactionalEmailAsync(malJetEmail);

            if (response.Messages.Length == 1)
            {
                Console.WriteLine("Success");
            }
        }
    }
}