using System.Net;
using System.Net.Mail;

namespace WpfApp1.Services
{
    /// <summary>
    /// Сервис для отправки писем через SMTP.
    /// Используется для отправки кодов подтверждения (2FA, восстановление пароля).
    /// </summary>
    public class EmailService
    {
        /* 
        Почта отправителя. Должна быть реальной почтой Mail.ru.
        Пароль приложения Mail.ru.
        Генерируется в настройках безопасности почты.
        Обычный пароль здесь НЕ работает.
        */
        private readonly string fromEmail = "test123111ergdf@mail.ru"; 
        private readonly string appPassword = "b5ZVuUJm0HnvvwHeRVc0";     

        public void SendCode(string toEmail, string code)
        {
            var msg = new MailMessage();
            msg.From = new MailAddress(fromEmail); // Адрес отправителя
            msg.To.Add(toEmail); // Адрес получателя
            msg.Subject = "Код подтверждения"; // Тема письма
            msg.Body = $"Ваш код: {code}"; // Текст письма
            msg.IsBodyHtml = false; // Письмо без HTML

            /*
            Настраиваем SMTP‑клиент.
            smtp.mail.ru — сервер Mail.ru
            587 — порт для защищённого соединения TLS
            */
            var smtp = new SmtpClient("smtp.mail.ru", 587);
            smtp.EnableSsl = true;
            // Авторизация через пароль приложения
            smtp.Credentials = new NetworkCredential(fromEmail, appPassword);

            smtp.Send(msg);
        }
    }
}
