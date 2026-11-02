using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public class Booking
    {
        private static int _lastBookingId;
        private readonly List<INotification> _notifications = new();

        public Booking(
            IRoom room,
            string guestName,
            string guestEmail,
            DateTime checkIn,
            DateTime checkOut,
            IPriceStrategy priceStrategy)
        {
            ArgumentNullException.ThrowIfNull(room);
            ArgumentNullException.ThrowIfNull(priceStrategy);

            if (string.IsNullOrWhiteSpace(guestName))
            {
                throw new ArgumentException("Укажите имя гостя.", nameof(guestName));
            }

            if (string.IsNullOrWhiteSpace(guestEmail))
            {
                throw new ArgumentException("Укажите адрес для уведомлений.", nameof(guestEmail));
            }

            if (checkOut.Date <= checkIn.Date)
            {
                throw new ArgumentException("Дата выезда должна быть позже даты заезда.", nameof(checkOut));
            }

            BookingId = Interlocked.Increment(ref _lastBookingId);
            Room = room;
            GuestName = guestName.Trim();
            GuestEmail = guestEmail.Trim();
            CheckIn = checkIn.Date;
            CheckOut = checkOut.Date;
            Nights = (CheckOut - CheckIn).Days;
            TotalPrice = priceStrategy.CalculatePrice(Room.BasePrice, Nights, CheckIn);
        }

        public int BookingId { get; }
        public IRoom Room { get; }
        public string GuestName { get; }
        public string GuestEmail { get; }
        public DateTime CheckIn { get; }
        public DateTime CheckOut { get; }
        public int Nights { get; }
        public decimal TotalPrice { get; }
        public bool IsConfirmed { get; private set; }
        public bool IsCancelled { get; private set; }

        /// <summary>Подписывает канал уведомлений на события этого бронирования (Observer).</summary>
        public void AttachNotification(INotification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);
            if (!_notifications.Contains(notification))
            {
                _notifications.Add(notification);
            }
        }

        public void DetachNotification(INotification notification)
        {
            _notifications.Remove(notification);
        }

        public void Confirm()
        {
            if (IsCancelled)
            {
                throw new InvalidOperationException("Нельзя подтвердить отменённое бронирование.");
            }

            if (IsConfirmed)
            {
                return;
            }

            IsConfirmed = true;
            Notify($"Бронирование №{BookingId} номера {Room.RoomNumber} подтверждено. " +
                   $"Период: {CheckIn:dd.MM.yyyy}-{CheckOut:dd.MM.yyyy}. Стоимость: {TotalPrice:N0} руб.");
        }

        public void Cancel()
        {
            if (IsCancelled)
            {
                return;
            }

            IsCancelled = true;
            Room.IsAvailable = true;
            Notify($"Бронирование №{BookingId} номера {Room.RoomNumber} отменено.");
        }

        private void Notify(string message)
        {
            foreach (var notification in _notifications)
            {
                notification.Send(message, GuestEmail);
            }
        }
    }
}
