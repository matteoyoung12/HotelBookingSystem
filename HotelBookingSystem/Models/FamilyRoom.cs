namespace HotelsManager
{
    /// <summary>Семейный номер для размещения до пяти гостей.</summary>
    public class FamilyRoom : RoomBase
    {
        public FamilyRoom(string roomNumber)
        {
            RoomNumber = roomNumber;
        }

        public override RoomType Type => RoomType.Family;
        public override decimal BasePrice => 6000m;
        public override int Capacity => 5;

        public override string GetDescription()
        {
            return $"Семейный номер {RoomNumber} с двумя спальнями";
        }
    }
}
