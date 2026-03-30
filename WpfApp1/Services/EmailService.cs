using System.Net;
using System.Net.Mail;

namespace WpfApp1.Services
{
    public class EmailService
    {
        private readonly string fromEmail = "test123111ergdf@mail.ru"; 
        private readonly string appPassword = "b5ZVuUJm0HnvvwHeRVc0";     

        public void SendCode(string toEmail, string code)
        {
            var msg = new MailMessage();
            msg.From = new MailAddress(fromEmail);
            msg.To.Add(toEmail);
            msg.Subject = "Код подтверждения";
            msg.Body = $"Ваш код: {code}";
            msg.IsBodyHtml = false;
            var smtp = new SmtpClient("smtp.mail.ru", 587);
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(fromEmail, appPassword);

            smtp.Send(msg);
        }
    }
}
