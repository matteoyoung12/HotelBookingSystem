using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public class EmailNotification : INotification
    {
        public NotificationType Type => NotificationType.Email;

        public bool Send(string message, string recipient)
        {
            // TODO: Реализовать логику отправки email
            Console.WriteLine($"📧 Email для {recipient}: {message}");
            return true;
        }
    }
}
