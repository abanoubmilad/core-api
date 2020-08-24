using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace core_api.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(Message message);
        Task SendEmailAsync(Message message, EmailConfig emailConfiguration);

        void SendEmail(Message message);
        void SendEmail(Message message, EmailConfig emailConfiguration);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailConfig _emailConfiguration;

        public EmailService(AppSettings appSettings)
        {
            _emailConfiguration = appSettings.EmailConfig;

        }

        public async Task SendEmailAsync(Message message)
        {
            await SendEmailAsync(message, _emailConfiguration);
        }
        public async Task SendEmailAsync(Message message, EmailConfig emailConfiguration)
        {
            await SendAsync(CreateEmailMessage(message, emailConfiguration.CredentialsEmail), emailConfiguration);
        }

        public void SendEmail(Message message)
        {
            SendEmail(message, _emailConfiguration);
        }
        public void SendEmail(Message message, EmailConfig emailConfiguration)
        {
            new Thread(() =>
            {
                Send(CreateEmailMessage(message, emailConfiguration.CredentialsEmail), emailConfiguration);
            }).Start();
        }

        private MimeMessage CreateEmailMessage(Message message, string from)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(from));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = string.Format("<h2 style='color:red;'>{0}</h2>", message.Content) };

            if (message.Attachments != null && message.Attachments.Any())
            {
                byte[] fileBytes;
                foreach (var attachment in message.Attachments)
                {
                    using (var ms = new MemoryStream())
                    {
                        attachment.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }

                    bodyBuilder.Attachments.Add(attachment.FileName, fileBytes, ContentType.Parse(attachment.ContentType));
                }
            }

            emailMessage.Body = bodyBuilder.ToMessageBody();
            return emailMessage;
        }

        private async Task SendAsync(MimeMessage mailMessage, EmailConfig emailConfiguration)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(emailConfiguration.SMTPServer, emailConfiguration.SMTPPort,
                        emailConfiguration.EnableSSL ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);

                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(emailConfiguration.CredentialsEmail, emailConfiguration.CredentialsPassword);

                    await client.SendAsync(mailMessage);
                }
                catch
                {
                    //log an error message or throw an exception, or both.
                    //throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }


        private void Send(MimeMessage mailMessage, EmailConfig emailConfiguration)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(emailConfiguration.SMTPServer, emailConfiguration.SMTPPort,
                        emailConfiguration.EnableSSL ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);

                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(emailConfiguration.CredentialsEmail, emailConfiguration.CredentialsPassword);

                    client.Send(mailMessage);
                }
                catch
                {
                    //log an error message or throw an exception, or both.
                    //throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }
    }
}
