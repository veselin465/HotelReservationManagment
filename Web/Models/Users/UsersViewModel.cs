using Data.Enumeration;
using System;

namespace Web.Models.Users
{
    public class UsersViewModel
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string EGN { get; set; }

        public string TelephoneNumber { get; set; }

        public string Email { get; set; }

        public bool IsActive { get; set; }

        public DateTime DateOfBeingHired { get; set; }

        public DateTime? DateOfBeingFired { get; set; }

    }
}
