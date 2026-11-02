using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public interface IPriceStrategy
    {
        decimal CalculatePrice(decimal basePrice, int days, DateTime checkInDate);
    }
}
