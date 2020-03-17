using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Enumeration;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models.Clients;
using Web.Models.Rooms;
using Web.Models.Users;

namespace Web.Models.Reservations
{
    public class ClientsFilterViewModel
    {

        [DataType(DataType.Text)]
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Names can only contain letters")]
        [StringLength(40, ErrorMessage = "Name cannot be longer than 40 characters")]
        public string FirstName { get; set; }


        [DataType(DataType.Text)]
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Names can only contain letter")]
        [StringLength(40, ErrorMessage = "Name cannot be longer than 40 characters")]
        public string LastName { get; set; }

    }
}
