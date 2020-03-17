using System.ComponentModel.DataAnnotations;

namespace Data.Enumeration
{
    public enum RoomTypeEnum
    {
        [Display(Name = "Two separated beds")]
        TwoSingleBed = 0,
        Apartment = 1,
        [Display(Name = "One double bed")]
        DoubleBed = 2,
        Penthouse = 3, 
        Maisonette = 4
    }
}
