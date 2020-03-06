using Data.Enumeration;
using System;

namespace Web.Models.Rooms
{
    public class RoomsViewModel
    {
        public int Id { get; set; }

        public int Number { get; set; }

        public int Capacity { get; set; }

        public bool IsFree { get; set; }

        public decimal PriceAdult { get; set; }

        public decimal PriceChild { get; set; }

        public RoomTypeEnum Type { get; set; }

    }
}
