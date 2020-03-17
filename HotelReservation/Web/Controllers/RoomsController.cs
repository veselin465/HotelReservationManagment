using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Entity;
using Web.Models.Rooms;
using Web.Models.Shared;
using Web.Models.Users;
using Web.Models.Reservations;
using Data.Enumeration;
using Web.Models.Validation;

namespace Web.Controllers
{
    public class RoomsController : Controller
    {
        private readonly int PageSize = GlobalVar.AmountOfElementsDisplayedPerPage;
        private readonly HotelReservationDb _context;

        public RoomsController()
        {
            _context = new HotelReservationDb();
        }

        public IActionResult ChangePageSize(int id)
        {
            if (id > 0)
            {
                GlobalVar.AmountOfElementsDisplayedPerPage = id;
            }

            return RedirectToAction("Index");
        }

        // GET: Rooms
        public IActionResult Index(RoomsIndexViewModel model)
        {

            UpdateRoomOccupacity();


            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            var contextDb = Filter(_context.Rooms.ToList(), model.Filter);

            List<RoomsViewModel> items = contextDb.Skip((model.Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(c => new RoomsViewModel()
            {
                Id = c.Id,
                Number = c.Number,
                PriceAdult = c.PriceAdult,
                PriceChild = c.PriceChild,
                Type = (RoomTypeEnum)c.Type,
                Capacity = c.Capacity,
                IsFree = c.IsFree
            }).ToList();

            model.Items = items;
            model.Pager.PagesCount = Math.Max(1, (int)Math.Ceiling(contextDb.Count() / (double)PageSize));

            return View(model);
        }

        // GET: Rooms/Create
        public IActionResult Create()
        {
            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.Admininstrator)
            {
                return RedirectToAction("LogInPermissionDenied", "Users");
            }

            return View(new RoomsCreateViewModel());
        }

        // POST: Rooms/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RoomsCreateViewModel createModel)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.Admininstrator)
            {
                return RedirectToAction("LogInPermissionDenied", "Users");
            }
            createModel.Message = null;
            if (ModelState.IsValid)
            {

                try
                {
                    Validate(new Validation_Room()
                    {
                        Capacity = createModel.Capacity,
                        Number = createModel.Number
                    });
                }
                catch (InvalidOperationException e)
                {
                    createModel.Message = e.Message;
                    return View(createModel);
                }



                if (_context.Rooms.Where(x => x.Number == createModel.Number).Count() > 0)
                {
                    createModel.Message = $"Room cant be created becuase there's already an existing room with the given number ({createModel.Number})";
                    return View(createModel);
                }

                Room room = new Room
                {
                    Number = createModel.Number,
                    PriceAdult = createModel.PriceAdult,
                    PriceChild = createModel.PriceChild,
                    Type = (int)createModel.RoomType,
                    Capacity = createModel.Capacity
                };

                _context.Rooms.Add(room);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
      
        // GET: Rooms/Edit/5
        public IActionResult Edit(int? id)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.Admininstrator)
            {
                return RedirectToAction("LogInPermissionDenied", "Users");
            }

            if (id == null || !RoomExists((int)id))
            {
                return NotFound();
            }

            Room room = _context.Rooms.Find(id);

            RoomsEditViewModel model = new RoomsEditViewModel
            {
                Id = room.Id,
                Number = room.Number,
                PriceAdult = room.PriceAdult,
                PriceChild = room.PriceChild,
                RoomType = (RoomTypeEnum)room.Type,
                Capacity = room.Capacity,
                IsFree = room.IsFree
            };

            return View(model);
        }

        // POST: Rooms/Edit/5       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RoomsEditViewModel editModel)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.Admininstrator)
            {
                return RedirectToAction("LogInPermissionDenied", "Users");
            }

            if (ModelState.IsValid)
            {

                if (!RoomExists(editModel.Id))
                {
                    return NotFound();
                }

                try
                {
                    Validate(new Validation_Room()
                    {
                        Capacity = editModel.Capacity,
                        Number = editModel.Number
                    });
                }
                catch (InvalidOperationException e)
                {
                    editModel.Message = e.Message;
                    return View(editModel);
                }

                Room room = new Room()
                {
                    Id = editModel.Id,
                    Number = editModel.Number,
                    PriceAdult = editModel.PriceAdult,
                    PriceChild = editModel.PriceChild,
                    Type = (int)editModel.RoomType,
                    Capacity = editModel.Capacity
                };

                _context.Update(room);
                _context.SaveChanges();

                UpdateAllReservationsOverallPriceRelatedToRoom(room.Id);

                return RedirectToAction(nameof(Index));
            }

            return View(editModel);
        }

        // GET: Rooms/Delete/5
        public IActionResult Delete(int? id)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.Admininstrator)
            {
                return RedirectToAction("LogInPermissionDenied", "Users");
            }

            if (id == null || !RoomExists((int)id))
            {
                return NotFound();
            }

            Room room = _context.Rooms.Find(id);
            _context.Rooms.Remove(room);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        private void UpdateRoomOccupacity()
        {
            foreach (var room in _context.Rooms.ToList())
            {
                var reservations = _context.Reservations.Where(x => x.RoomId == room.Id);
                bool isFree = true;
                foreach (var reservation in reservations)
                {
                    if (reservation.DateOfAccommodation.AddHours(GlobalVar.DefaultReservationHourStart) < DateTime.UtcNow && DateTime.UtcNow < reservation.DateOfExemption.AddHours(GlobalVar.DefaultReservationHourStart))
                    {
                        isFree = false;
                        break;
                    }
                }
                room.IsFree = isFree;
                _context.Rooms.Update(room);
                _context.SaveChanges();
            }

        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }

        private void Validate(Validation_Room model)
        {

            if (model.Number<=0)
            {
                throw new InvalidOperationException("Room number must be positive integer");
            }

            if (model.Capacity <= 0)
            {
                throw new InvalidOperationException("Room capacity must be positive integer");
            }

        }

        private List<Room> Filter(List<Room> collection, RoomsFilterViewModel filterModel)
        {

            if (filterModel != null)
            {
                if (filterModel.Capacity != null)
                {
                    collection = collection.Where(x => x.Capacity == filterModel.Capacity).ToList();
                }
                if (filterModel.Type != null)
                {
                    collection = collection.Where(x => x.Type == (int)filterModel.Type).ToList();
                }
                if (filterModel.IsFree != null)
                {
                    collection = collection.Where(x => x.IsFree == filterModel.IsFree).ToList();
                }

            }

            return collection;
        }

        private void UpdateAllReservationsOverallPriceRelatedToRoom(int roomId)
        {
            List<Reservation> reservations = _context.Reservations.Where(x => x.RoomId == roomId).ToList();

            foreach (var reservation in reservations)
            {
                int days = CalculateDaysPassed(reservation.DateOfAccommodation, reservation.DateOfExemption);
                List<int> clientsId = _context.ClientReservation.Where(x => x.ReservationId == reservation.Id).Select(x => x.ClientId).ToList();
                decimal bill = 0;
                Room room = _context.Rooms.Find(reservation.RoomId);
                foreach (var clientId in clientsId)
                {
                    bill += (_context.Clients.Find(clientId).IsAdult) ? (room.PriceAdult) : (room.PriceChild);
                }
                bill = AddExtras(bill, reservation.IsAllInclusive, reservation.IsBreakfastIncluded);
                reservation.OverallBill = bill;
                _context.Reservations.Update(reservation);
                _context.SaveChanges();
            }

        }

        private int CalculateDaysPassed(DateTime startDate, DateTime endDate)
        {
            double daysDiffDouble = (endDate - startDate).TotalDays;

            int daysDiff = (int)daysDiffDouble;
            if (daysDiffDouble > daysDiff)
            {
                daysDiff++;
            }

            return daysDiff;

        }

        private decimal AddExtras(decimal money, bool isAllInclusive, bool isBreakfastIncluded)
        {
            decimal bonusPercentage = 0;
            if (isAllInclusive)
            {
                bonusPercentage += GlobalVar.AllInclusiveExtraBillPercentage;
            }
            if (isBreakfastIncluded)
            {
                bonusPercentage += GlobalVar.InlcludedBreakfastExtraBillPercentage;
            }
            return money * (1 + bonusPercentage / 100);
        }

    }
}
