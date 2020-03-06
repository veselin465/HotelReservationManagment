using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Enumeration;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models.Clients;
using Web.Models.Rooms;
using Web.Models.Users;

namespace Web.Models.Reservations
{
    public class RoomsFilterViewModel
    {

        public int? ReservedCount { get; set; }

        public int? Capacity { get; set; }

        public RoomTypeEnum? Type { get; set; }

    }
}
