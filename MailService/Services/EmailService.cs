using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

   public async Task SendEmailAsync(string toEmailAddress, string subject, string body)
{
    var smtpSettings = _configuration.GetSection("Smtp");
    var smtpServer = smtpSettings["Server"] ?? throw new InvalidOperationException("SMTP server address is missing.");
    var smtpPortString = smtpSettings["Port"];
    var username = smtpSettings["Username"] ?? throw new InvalidOperationException("SMTP username is missing.");
    var password = smtpSettings["Password"] ?? throw new InvalidOperationException("SMTP password is missing.");

    if (!int.TryParse(smtpPortString, out var smtpPort))
    {
        throw new InvalidOperationException("SMTP port is not a valid integer.");
    }

    var message = new MimeMessage();
    message.From.Add(new MailboxAddress("Your Name", username));
    message.To.Add(new MailboxAddress("", toEmailAddress));
    message.Subject = subject;

    var bodyBuilder = new BodyBuilder { TextBody = body };
    message.Body = bodyBuilder.ToMessageBody();

    try
    {
        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
    catch (SmtpCommandException smtpEx)
    {
        // SMTP komut hatası, genellikle hatanın nedeni burada belirtilir.
        throw new InvalidOperationException("SMTP Command Error: " + smtpEx.Message, smtpEx);
    }
    catch (SmtpProtocolException smtpEx)
    {
        // SMTP protokol hatası
        throw new InvalidOperationException("SMTP Protocol Error: " + smtpEx.Message, smtpEx);
    }
    catch (Exception ex)
    {
        // Genel hata
        throw new InvalidOperationException("An error occurred while sending the email.", ex);
    }
}
}