using System.Collections;
using System.Collections.Generic;
using Web.Models.Reservations;
using Web.Models.Shared;

namespace Web.Models.Users
{
    public class UsersIndexViewModel
    {
        public PagerViewModel Pager { get; set; }

        public UsersFilterViewModel Filter { get; set; }

        public ICollection<UsersViewModel> Items { get; set; }
    }
}
