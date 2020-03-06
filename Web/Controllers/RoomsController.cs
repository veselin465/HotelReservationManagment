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
        public async Task<IActionResult> Index(RoomsIndexViewModel model)
        {

            UpdateRoomOccupacity();


            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            var contextDb = Filter(await _context.Rooms.ToListAsync(), model.Filter);

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
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Rooms.CountAsync() / (double)PageSize);

            return View(model);
        }

        // GET: Rooms/Create
        public IActionResult Create()
        {
            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.admininstration)
            {
                return RedirectToAction("LogInPermissionDenied", "Users");
            }

            RoomsCreateViewModel model = new RoomsCreateViewModel();

            return View(model);
        }

        // POST: Rooms/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomsCreateViewModel createModel)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.admininstration)
            {
                return RedirectToAction("LogInPermissionDenied", "Users");
            }

            if (ModelState.IsValid)
            {

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

                _context.Add(room);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(createModel);
        }

        // GET: Rooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.admininstration)
            {
                return RedirectToAction("LogInPermissionDenied", "Users");
            }

            if (id == null)
            {
                return NotFound();
            }

            Room room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

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
        public async Task<IActionResult> Edit(RoomsEditViewModel editModel)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.admininstration)
            {
                return RedirectToAction("LogInPermissionDenied", "Users");
            }

            if (ModelState.IsValid)
            {
                Room room = new Room()
                {
                    Id = editModel.Id,
                    Number = editModel.Number,
                    PriceAdult = editModel.PriceAdult,
                    PriceChild = editModel.PriceChild,
                    Type = (int)editModel.RoomType,
                    Capacity = editModel.Capacity,
                    IsFree = editModel.IsFree
                };

                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                UpdateAllReservationsOverallPriceRelatedToRoom(room.Id);

                return RedirectToAction(nameof(Index));
            }

            return View(editModel);
        }

        // GET: Rooms/Delete/5
        public async Task<IActionResult> Delete(int id)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.admininstration)
            {
                return RedirectToAction("LogInPermissionDenied", "Users");
            }

            Room room = await _context.Rooms.FindAsync(id);
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

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
                    if (reservation.DateOfAccommodation.AddHours(12) < DateTime.UtcNow && DateTime.UtcNow < reservation.DateOfExemption.AddHours(12))
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

        private List<Room> Filter(List<Room> collection, RoomsFilterViewModel filterModel)
        {

            if (filterModel != null)
            {
                if (filterModel.Capacity != null)
                {
                    collection = collection.Where(x => x.Capacity == filterModel.Capacity).ToList();
                }
                /*if (filterModel.ReservedCount != null)
                {
                    collection = collection.Where(x => x.C == filterModel.ReservedCount).ToList();
                }*/
                if (filterModel.Type != null)
                {
                    collection = collection.Where(x => x.Type == (int)filterModel.Type).ToList();
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
                bonusPercentage += GlobalVar.InlcludedBreakfastBonusExtraBillPercentage;
            }
            return money * (1 + bonusPercentage / 100);
        }

    }
}
