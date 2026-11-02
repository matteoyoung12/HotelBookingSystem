namespace HotelsManager
{
    internal static class PriceValidation
    {
        internal static void Validate(decimal basePrice, int days)
        {
            if (basePrice < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(basePrice), "Базовая цена не может быть отрицательной.");
            }

            if (days <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(days), "Количество ночей должно быть больше нуля.");
            }
        }
    }
}
