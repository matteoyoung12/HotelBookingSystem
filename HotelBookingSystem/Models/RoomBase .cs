using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public abstract class RoomBase : IRoom
    {
        public string RoomNumber { get; protected set; } = string.Empty;
        public abstract RoomType Type { get; }
        public abstract decimal BasePrice { get; }
        public abstract int Capacity { get; }
        public bool IsAvailable { get; set; } = true;

        public abstract string GetDescription();

        public virtual decimal CalculateTotalPrice(int days)
        {
            if (days <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(days), "Количество ночей должно быть больше нуля.");
            }

            return BasePrice * days;
        }
    }
}
