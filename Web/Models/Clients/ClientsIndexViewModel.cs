using System.Collections;
using System.Collections.Generic;
using Web.Models.Reservations;
using Web.Models.Shared;

namespace Web.Models.Clients
{
    public class ClientsIndexViewModel
    {
        public PagerViewModel Pager { get; set; }

        public ClientsFilterViewModel Filter { get; set; }

        public ICollection<ClientsViewModel> Items { get; set; }
    }
}
