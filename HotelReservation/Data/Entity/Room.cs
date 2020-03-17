using Data.Enumeration;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.Entity
{
    public class Room
    {

        public Room()
        {
            IsFree = true;
            this.Reservations = new List<Reservation>();
        }

        public int Id { get; set; }


        [Required]
        [Display(Name = "Room number")]
        public int Number { get; set; }


        [Required]
        [Display(Name = "Room capacity")]
        public int Capacity { get; set; }


        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Price (Adult)")]
        public decimal PriceAdult { get; set; }


        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Price (child)")]
        public decimal PriceChild { get; set; }


        [Required]
        public int Type { get; set; }


        public bool IsFree { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }

    }
}
