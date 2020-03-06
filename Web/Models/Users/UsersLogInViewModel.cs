using System.ComponentModel.DataAnnotations;
using Data.Enumeration;

namespace Web.Models.Users
{
    public class UsersLogInViewModel
    {
        /*[Required]
        [StringLength(50, ErrorMessage = "add-error-message")]*/

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string Message { get; set; }

    }
}
