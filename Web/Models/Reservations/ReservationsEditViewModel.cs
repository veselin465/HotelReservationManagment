using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Enumeration;
using Microsoft.AspNetCore.Mvc;
using Web.Models.Clients;
using Web.Models.Rooms;
using Web.Models.Users;

namespace Web.Models.Reservations
{
    public class ReservationsEditViewModel
    {
        /*[Required]
        [StringLength(50, ErrorMessage = "add-error-message")]*/

        [HiddenInput]
        public int Id { get; set; }

        [Required]
        public DateTime DateOfAccommodation { get; set; }
        [Required]
        public DateTime DateOfExemption { get; set; }

        [Required]
        public bool IsBreakfastIncluded { get; set; }
        [Required]
        public bool IsAllInclusive { get; set; }

        public decimal OverallBill { get; set; }

        public string Message { get; set; }

    }
}
