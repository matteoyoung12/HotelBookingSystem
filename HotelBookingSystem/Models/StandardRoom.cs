using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public class StandardRoom : RoomBase
    {
        public StandardRoom(string roomNumber)
        {
            RoomNumber = roomNumber;
        }

        public override RoomType Type => RoomType.Standard;

        public override decimal BasePrice => 2500m;

        public override int Capacity => 2;

        public override string GetDescription()
        {
            return $"Стандартный номер {RoomNumber}";
        }
    }
}
