using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelsManager
{
    public class RoomFactory
    {
        public IRoom CreateStandardRoom(string roomNumber)
        {
            return CreateRoom(RoomType.Standard, roomNumber);
        }

        public IRoom CreateDeluxeRoom(string roomNumber)
        {
            return CreateRoom(RoomType.Deluxe, roomNumber);
        }

        public IRoom CreateSuite(string roomNumber)
        {
            return CreateRoom(RoomType.Suite, roomNumber);
        }

        public IRoom CreateFamilyRoom(string roomNumber)
        {
            return CreateRoom(RoomType.Family, roomNumber);
        }

        /// <summary>Factory Method: создаёт конкретный тип номера по его значению.</summary>
        public IRoom CreateRoom(RoomType roomType, string roomNumber)
        {
            if (string.IsNullOrWhiteSpace(roomNumber))
            {
                throw new ArgumentException("Номер комнаты не может быть пустым.", nameof(roomNumber));
            }

            return roomType switch
            {
                RoomType.Standard => new StandardRoom(roomNumber),
                RoomType.Deluxe => new DeluxeRoom(roomNumber),
                RoomType.Suite => new Suite(roomNumber),
                RoomType.Family => new FamilyRoom(roomNumber),
                _ => throw new ArgumentOutOfRangeException(nameof(roomType), roomType, "Неизвестный тип номера.")
            };
        }
    }
}
