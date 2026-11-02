using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public class HotelManager
    {
        private readonly List<IRoom> _rooms = new();
        private readonly List<Booking> _bookings = new();
        private readonly NotificationManager _notificationManager;

        public HotelManager(NotificationManager notificationManager)
        {
            _notificationManager = notificationManager;
        }

        public void AddRoom(IRoom room)
        {
            ArgumentNullException.ThrowIfNull(room);
            if (_rooms.Any(existing => string.Equals(existing.RoomNumber, room.RoomNumber, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"Номер {room.RoomNumber} уже добавлен в отель.", nameof(room));
            }

            _rooms.Add(room);
        }

        public void RemoveRoom(string roomNumber)
        {
            var room = _rooms.SingleOrDefault(room =>
                string.Equals(room.RoomNumber, roomNumber, StringComparison.OrdinalIgnoreCase));

            if (room is null)
            {
                throw new KeyNotFoundException($"Номер {roomNumber} не найден.");
            }

            if (!room.IsAvailable)
            {
                throw new InvalidOperationException("Нельзя удалить номер с активным бронированием. Сначала отмените бронь.");
            }

            _rooms.Remove(room);
        }

        public List<IRoom> FindAvailableRooms()
        {
            return _rooms.Where(room => room.IsAvailable).ToList();
        }

        public List<IRoom> FindAvailableRooms(RoomType roomType)
        {
            return _rooms.Where(room => room.IsAvailable && room.Type == roomType).ToList();
        }

        /// <summary>Поиск по типу, минимальной вместимости и максимальной цене за ночь.</summary>
        public List<IRoom> FindAvailableRooms(RoomType? roomType, int? minimumCapacity, decimal? maximumPrice)
        {
            return _rooms.Where(room =>
                    room.IsAvailable &&
                    (!roomType.HasValue || room.Type == roomType.Value) &&
                    (!minimumCapacity.HasValue || room.Capacity >= minimumCapacity.Value) &&
                    (!maximumPrice.HasValue || room.BasePrice <= maximumPrice.Value))
                .ToList();
        }

        public Booking CreateBooking(string roomNumber, string guestName, string guestEmail, 
                                   DateTime checkIn, DateTime checkOut, IPriceStrategy priceStrategy)
        {
            var room = _rooms.SingleOrDefault(room =>
                string.Equals(room.RoomNumber, roomNumber, StringComparison.OrdinalIgnoreCase));

            if (room is null)
            {
                throw new KeyNotFoundException($"Номер {roomNumber} не найден.");
            }

            if (!room.IsAvailable)
            {
                throw new InvalidOperationException($"Номер {room.RoomNumber} уже забронирован.");
            }

            var booking = new Booking(room, guestName, guestEmail, checkIn, checkOut, priceStrategy);
            foreach (var notification in _notificationManager.Notifications)
            {
                booking.AttachNotification(notification);
            }

            room.IsAvailable = false;
            _bookings.Add(booking);
            return booking;
        }

        public void ShowHotelStatus()
        {
            Console.WriteLine($"Всего номеров: {_rooms.Count}");
            Console.WriteLine($"Свободных номеров: {_rooms.Count(room => room.IsAvailable)}");
            Console.WriteLine($"Активных бронирований: {_bookings.Count(booking => !booking.IsCancelled)}");
        }

        public Dictionary<RoomType, int> GetRoomStatistics()
        {
            return _rooms
                .GroupBy(room => room.Type)
                .ToDictionary(group => group.Key, group => group.Count());
        }
    }
}
