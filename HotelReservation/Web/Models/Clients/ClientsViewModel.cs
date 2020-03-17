using Data.Enumeration;
using System;

namespace Web.Models.Clients
{
    public class ClientsViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string TelephoneNumber { get; set; }

        public string Email { get; set; }

        public bool IsAdult { get; set; }

    }
}
