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
    public class UsersFilterViewModel
    {


        [StringLength(20, ErrorMessage = "Username can not be longer than 20 characters")]
        public string Username { get; set; }


        [DataType(DataType.Text)]
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Names can only contain letters")]
        [StringLength(40, ErrorMessage = "Name cannot be longer than 40 characters")]
        [Display(Name = "First name")]
        public string FirstName { get; set; }


        [DataType(DataType.Text)]
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Names can only contain letters")]
        [StringLength(40, ErrorMessage = "Name cannot be longer than 40 characters")]
        [Display(Name = "Middle name")]
        public string MiddleName { get; set; }


        [DataType(DataType.Text)]
        [RegularExpression(@"^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Names can only contain letter")]
        [StringLength(40, ErrorMessage = "Name cannot be longer than 40 characters")]
        [Display(Name = "Last name")]
        public string LastName { get; set; }


        [DataType(DataType.Text)]
        public string Email { get; set; }

    }
}
