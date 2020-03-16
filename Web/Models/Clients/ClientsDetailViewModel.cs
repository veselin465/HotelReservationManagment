using Data.Enumeration;
using System;
using System.Collections;
using System.Collections.Generic;
using Web.Models.Reservations;

namespace Web.Models.Clients
{
    public class ClientsDetailViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string TelephoneNumber { get; set; }

        public string Email { get; set; }

        public bool IsAdult { get; set; }

        public ICollection<ReservationsViewModel> PastReservations { get; set; }

        public ICollection<ReservationsViewModel> UpcomingReservations { get; set; }

    }
}
