using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{

    public interface IRoom
    {
        string RoomNumber { get; }
        RoomType Type { get; }
        decimal BasePrice { get; }
        int Capacity { get; }
        bool IsAvailable { get; set; }
        string GetDescription();
    }
}
