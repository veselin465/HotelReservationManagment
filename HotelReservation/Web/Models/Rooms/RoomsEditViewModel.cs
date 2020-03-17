using System.ComponentModel.DataAnnotations;
using Data.Enumeration;
using Microsoft.AspNetCore.Mvc;

namespace Web.Models.Rooms
{
    public class RoomsEditViewModel
    {

        /*[Required]
        [StringLength(50, ErrorMessage = "Brand name should be at most 50 characters")]*/

        [HiddenInput]
        public int Id { get; set; }

        [Required]
        public int Number { get; set; }

        [Required]
        public int Capacity { get; set; }

        public bool IsFree { get; set; }

        [Required]
        [Range(0.01, 9999.00, ErrorMessage = "Bed price should be between 0.01 and 9999.00")]
        public decimal PriceAdult { get; set; }

        [Required]
        [Range(0.01, 9999.00, ErrorMessage = "Bed price should be between 0.01 and 9999.00")]
        public decimal PriceChild { get; set; }

        [Required]
        public RoomTypeEnum RoomType { get; set; }

        public string Message { get; set; }

    }
}
