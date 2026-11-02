using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public class SeasonalPriceStrategy : IPriceStrategy
    {
        public decimal CalculatePrice(decimal basePrice, int days, DateTime checkInDate)
        {
            PriceValidation.Validate(basePrice, days);

            decimal total = 0;
            for (var night = 0; night < days; night++)
            {
                var date = checkInDate.Date.AddDays(night);
                var multiplier = date.Month switch
                {
                    6 or 7 or 8 => 1.25m,   // летний сезон
                    12 or 1 => 1.30m,       // новогодний сезон
                    _ => 1m
                };
                total += basePrice * multiplier;
            }

            return total;
        }
    }
}
