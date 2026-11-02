using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public class WeekendPriceStrategy : IPriceStrategy
    {
        public decimal CalculatePrice(decimal basePrice, int days, DateTime checkInDate)
        {
            PriceValidation.Validate(basePrice, days);

            decimal total = 0;
            for (var night = 0; night < days; night++)
            {
                var date = checkInDate.Date.AddDays(night);
                var isWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                total += basePrice * (isWeekend ? 1.20m : 1m);
            }

            return total;
        }
    }
}
