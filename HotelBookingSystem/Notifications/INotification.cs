using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public interface INotification
    {
        bool Send(string message, string recipient);
        NotificationType Type { get; }
    }
}
