using Data.Enumeration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entity
{
    public class User
    {

        public User()
        {
            this.IsActive = true;
            this.DateOfBeingFired = null;
            this.Reservations = new List<Reservation>();
        }

        public int Id { get; set; }


        [Required]
        [StringLength(20, ErrorMessage = "Username can not be longer than 20 characters")]
        public string Username { get; set; }


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        [Required]
        [DataType(DataType.Text)]
        [StringLength(40, ErrorMessage = "Name can not be longer than 40 characters")]
        public string FirstName { get; set; }


        [Required]
        [StringLength(40, ErrorMessage = "Name can not be longer than 40 characters")]
        public string MiddleName { get; set; }


        [Required]
        [StringLength(40, ErrorMessage = "Name can not be longer than 40 characters")]
        public string LastName { get; set; }


        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "EGN must be exactly 10 characters long.")]
        public string EGN { get; set; }


        [Required]
        [DataType(DataType.PhoneNumber)]
        [StringLength(10, ErrorMessage = "Phone number cannot be longer than 10 characters)")]
        public string TelephoneNumber { get; set; }


        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Hire date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DateOfBeingHired { get; set; } = DateTime.UtcNow;


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBeingFired { get; set; }


        public bool IsActive { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }


    }
}