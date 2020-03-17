using System.Collections;
using System.Collections.Generic;
using Web.Models.Reservations;
using Web.Models.Shared;

namespace Web.Models.Rooms
{
    public class RoomsIndexViewModel
    {
        public PagerViewModel Pager { get; set; }

        public RoomsFilterViewModel Filter {get;set;}

        public ICollection<RoomsViewModel> Items { get; set; }
    }
}
