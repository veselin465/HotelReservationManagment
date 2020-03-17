using Data.Enumeration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entity
{
    public static class GlobalVar
    {

        public const int InlcludedBreakfastExtraBillPercentage = 5;

        public const int AllInclusiveExtraBillPercentage = 12;

        public const int DefaultReservationHourStart = 12;

        public static int AmountOfElementsDisplayedPerPage { set; get; } = 10;

        public static int LoggedOnUserId { set; get; } = -1;
        public enum UserRights
        {
            Admininstrator,
            DefaultUser
        }
        public static UserRights LoggedOnUserRights { set; get; } = UserRights.DefaultUser;


    }
}