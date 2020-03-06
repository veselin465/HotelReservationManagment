using System.ComponentModel.DataAnnotations;
using Data.Enumeration;

namespace Web.Models.Clients
{
    public class ClientsCreateViewModel
    {
        /*[Required]
        [StringLength(50, ErrorMessage = "add-error-message")]*/


        [DataType(DataType.Text)]
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Use letters only please")]
        [StringLength(40, ErrorMessage = "Name must be no longer than 40 characters")]
        [Display(Name = "First name")]
        public string FirstName { get; set; }


        [DataType(DataType.Text)]
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Use letters only please")]
        [StringLength(40, ErrorMessage = "Name must be no longer than 40 characters")]
        [Display(Name = "Last name")]
        public string LastName { get; set; }


        [Required]
        [DataType(DataType.PhoneNumber)]
        [StringLength(10, ErrorMessage = "Phone number cannot be longer than 10 characters)")]
        public string TelephoneNumber { get; set; }


        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public bool IsAdult { get; set; }

    }
}
