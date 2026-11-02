using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public class DeluxeRoom : RoomBase
    {
        public DeluxeRoom(string roomNumber)
        {
            RoomNumber = roomNumber;
        }

        public override RoomType Type => RoomType.Deluxe;
        public override decimal BasePrice => 4500m;
        public override int Capacity => 3;

        public override string GetDescription()
        {
            return $"Делюкс номер {RoomNumber} с видом на море";
        }
    }
}
