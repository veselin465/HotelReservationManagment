using System.ComponentModel.DataAnnotations;
using Data.Enumeration;
using Microsoft.AspNetCore.Mvc;

namespace Web.Models.Users
{
    public class UsersEditViewModel
    {
        /*[Required]
        [StringLength(50, ErrorMessage = "add-error-message")]*/

        [HiddenInput]
        public int Id { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Username can not be longer than 20 characters")]
        public string Username { get; set; }

        public string Password { get; set; }


        [Required]
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Use letters only please")]
        [StringLength(40, ErrorMessage = "Name can not be longer than 40 characters")]
        public string FirstName { get; set; }


        [Required]
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Use letters only please")]
        [StringLength(40, ErrorMessage = "Name can not be longer than 40 characters")]
        public string MiddleName { get; set; }


        [Required]
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Use letters only please")]
        [StringLength(40, ErrorMessage = "Name can not be longer than 40 characters")]
        public string LastName { get; set; }


        [Required]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Use digits only please")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "EGN must be exactly 10 characters long.")]
        public string EGN { get; set; }


        [Required]
        [DataType(DataType.PhoneNumber)]
        [StringLength(10, ErrorMessage = "Phone number cannot be longer than 10 characters)")]
        public string TelephoneNumber { get; set; }


        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        public string Message { get; set; }

    }
}
