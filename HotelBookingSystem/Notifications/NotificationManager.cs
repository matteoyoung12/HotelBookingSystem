using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public class NotificationManager
    {
        private readonly List<INotification> _notifications = new();

        public IReadOnlyCollection<INotification> Notifications => _notifications.AsReadOnly();

        public void AddNotification(INotification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);

            if (!_notifications.Contains(notification))
            {
                _notifications.Add(notification);
            }
        }

        public void RemoveNotification(INotification notification)
        {
            _notifications.Remove(notification);
        }

        public void Broadcast(string message, string recipient)
        {
            foreach (var notification in _notifications)
            {
                notification.Send(message, recipient);
            }
        }
    }
}
