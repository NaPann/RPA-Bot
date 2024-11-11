using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SNJ.RPA.App
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public Message(IEnumerable<string> to, string subject, string content)
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress(x, x)));
            Subject = subject;
            Content = content;
        }
    }
    public interface IEmailSender
    {
        Task SendEmailAsync(Message message, string mailFrom);
    }
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(Message message, string mailFrom)
        {
            var mailMessage = CreateEmailMessage(message, mailFrom);

            await SendAsync(mailMessage);
        }
        string SmtpServer = "smtp.office365.com";
        int Port = 587;
        string Username = "noreply@snjinter.com";
        string Password = "Sji@2023";
        private async Task SendAsync(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(SmtpServer, Port, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(Username, Password);

                    await client.SendAsync(mailMessage);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
        private MimeMessage CreateEmailMessage(Message message, string mailFrom)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(mailFrom, Username));

            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = message.Content };
            emailMessage.Body = bodyBuilder.ToMessageBody();
            return emailMessage;
        }
    }
}
