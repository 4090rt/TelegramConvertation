using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace TelegramConvertorBots.SendEmail
{
    public class Send
    {
        private readonly string _smptServer = "smtp.gmail.com";
        private readonly int _port = 587;
        private readonly string _gmail = "artem2007yannurow@gmail.com";
        private readonly string _passwordmail = "qpco yszb yfjotetm";

        public async Task SmptServerSend(string Email, string docpath)
        {
            try
            {
                using (var smpt = new SmtpClient(_smptServer, _port))
                {
                    smpt.Credentials = new NetworkCredential(_gmail, _passwordmail);
                    smpt.EnableSsl = true;

                    var messagesmpt = new MailMessage
                    {
                        From = new MailAddress(Email),
                        Subject = "Ваш файл уже готов!",
                        Body = $"Файл: {System.IO.Path.GetFileName(docpath)}\n\nОтправлено: {DateTime.Now}",
                        IsBodyHtml = false
                    };
                    messagesmpt.To.Add(Email);

                    using (var attachment = new Attachment(docpath))
                    {
                        messagesmpt.Attachments.Add(attachment);
                        await smpt.SendMailAsync(messagesmpt);
                    }

                };
            }   
            catch(Exception ex) 
            {
                MessageBox.Show("Возникло исключение в FileSaveMail -> MailSendGmail -> smptserververifi" + ex.Message);
            }
        }
    }
}
