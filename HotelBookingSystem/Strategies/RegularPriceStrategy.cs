using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public class RegularPriceStrategy : IPriceStrategy
    {
        public decimal CalculatePrice(decimal basePrice, int days, DateTime checkInDate)
        {
            PriceValidation.Validate(basePrice, days);

            var total = basePrice * days;
            // Скидка за длительное проживание: 5 % от 7 ночей, 10 % от 14 ночей.
            var discount = days >= 14 ? 0.10m : days >= 7 ? 0.05m : 0m;
            return total * (1 - discount);
        }
    }
}
