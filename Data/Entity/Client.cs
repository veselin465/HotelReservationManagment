using Data.Enumeration;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entity
{
    public class Client
    {

        public Client()
        {
            this.Reservations = new List<ClientReservation>();
        }

        public int Id { get; set; }


        [Required]
        [DataType(DataType.Text)]
        [StringLength(40, ErrorMessage = "Name must be no longer than 40 characters")]
        [Display(Name = "First name")]
        public string FirstName { get; set; }


        [Required]
        [DataType(DataType.Text)]
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

        [Required]
        public bool IsAdult { get; set; }

        public virtual ICollection<ClientReservation> Reservations { get; set; }

    }
}