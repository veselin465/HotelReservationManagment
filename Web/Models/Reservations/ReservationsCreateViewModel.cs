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
    public class ReservationsCreateViewModel
    {
        /*[Required]
        [StringLength(50, ErrorMessage = "add-error-message")]*/



        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateOfAccommodation { get; set; } = DateTime.UtcNow;


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateOfExemption { get; set; } = DateTime.UtcNow;


        public bool IsBreakfastIncluded { get; set; }
        public bool IsAllInclusive { get; set; }
        public decimal OverallBill { get; set; }



        public int RoomId { get; set; }
        public int UserId { get; set; }

        public IEnumerable<SelectListItem> Rooms { get; set; }

        public IEnumerable<SelectListItem> Users { get; set; }

        public string Message { get; set; }

    }
}
