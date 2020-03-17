using Data.Entity;
using Data.Enumeration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Web.Models.Clients;
using Web.Models.Rooms;
using Web.Models.Users;

namespace Web.Models.Validation
{
    public class Validation_Reservation
    {
        
        public int RoomId { get; set; }
        
        public int ReservationId { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfAccommodation { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfExemption { get; set; }

    }
}
