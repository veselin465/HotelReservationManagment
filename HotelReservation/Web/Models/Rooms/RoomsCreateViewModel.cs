using System.ComponentModel.DataAnnotations;
using Data.Enumeration;

namespace Web.Models.Rooms
{
    public class RoomsCreateViewModel
    {
        /*[Required]
        [StringLength(50, ErrorMessage = "add-error-message")]*/

        [Required]
        public int Number { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required]
        [Range(0.01,9999.00,ErrorMessage = "Bed price should be between 0.01 and 9999.00")]
        public decimal PriceAdult { get; set; }

        [Required]
        [Range(0.01, 9999.00, ErrorMessage = "Bed price should be between 0.01 and 9999.00")]
        public decimal PriceChild { get; set; }

        [Required]
        public RoomTypeEnum RoomType { get; set; }

        public string Message { get; set; }
    }
}
