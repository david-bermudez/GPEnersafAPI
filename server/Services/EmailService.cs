using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace GpEnerSaf.Services
{
    public interface IEmailService
    {
        void Send(string from, string to, string subject, string html);
    }

    public class EmailService : IEmailService
    {
        public void Send(string from, string to, string subject, string html)
        {
            // TODO: Configuración solo para test...
            // to = "cacarmina@collahuasi.cl";
            if (to.IndexOf(";") > 0) to = to.Substring(0, to.IndexOf(";"));
            var host = Startup.StaticConfig.GetSection("EmailService").GetSection("Host").Value;
            var port = Startup.StaticConfig.GetSection("EmailService").GetSection("Port").Value;
            var user = Startup.StaticConfig.GetSection("EmailService").GetSection("User").Value;
            var pass = Startup.StaticConfig.GetSection("EmailService").GetSection("Pass").Value;
            
            var _from = Startup.StaticConfig.GetSection("EmailService").GetSection("From").Value;
            // TODO: Configuración solo para test...
            from = _from;
            
            // create message
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(from));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = html };

            // send email
            using var smtp = new SmtpClient();
            smtp.Connect(host, int.Parse(port), SecureSocketOptions.None);
            if (!string.IsNullOrEmpty(pass)) smtp.Authenticate(user, pass);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}