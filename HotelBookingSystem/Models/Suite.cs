using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public class Suite : RoomBase
    {
        public Suite(string roomNumber)
        {
            RoomNumber = roomNumber;
        }

        public override RoomType Type => RoomType.Suite;
        public override decimal BasePrice => 7500m;
        public override int Capacity => 4;

        public override string GetDescription()
        {
            return $"Люкс номер {RoomNumber} с гостиной и джакузи";
        }
    }
}
