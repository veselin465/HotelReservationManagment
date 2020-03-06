using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Entity;
using Web.Models.Reservations;
using Web.Models.Shared;
using Web.Models.Users;
using Web.Models.Rooms;
using Web.Models.Clients;
using Microsoft.AspNetCore.Mvc.Rendering;
using Data.Enumeration;

namespace Web.Controllers
{
    public class ReservationsController : Controller
    {

        private readonly int PageSize = GlobalVar.AmountOfElementsDisplayedPerPage;

        private readonly HotelReservationDb _context;

        public ReservationsController()
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

        // GET: Reservations
        public async Task<IActionResult> Index(ReservationsIndexViewModel model)
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            List<Reservation> reservations = await _context.Reservations.Skip((model.Pager.CurrentPage - 1) * PageSize).Take(PageSize).ToListAsync();

            List<ReservationsViewModel> list = new List<ReservationsViewModel>();

            foreach (var reservation in reservations)
            {
                int userId = reservation.UserId;
                int roomId = reservation.RoomId;

                UsersViewModel userVM = new UsersViewModel()
                {
                    Id = reservation.User.Id,
                    FirstName = reservation.User.FirstName,
                    LastName = reservation.User.LastName,
                };

                RoomsViewModel roomVM = new RoomsViewModel()
                {
                    Id = reservation.Room.Id,
                    Capacity = reservation.Room.Capacity,
                    PriceAdult = reservation.Room.PriceAdult,
                    PriceChild = reservation.Room.PriceChild,
                    Number = reservation.Room.Number,
                    Type = (RoomTypeEnum)reservation.Room.Type
                };

                int clientsCount = _context.ClientReservation.Where(x => x.ReservationId == reservation.Id).Count();

                list.Add(new ReservationsViewModel()
                {
                    Id = reservation.Id,
                    User = userVM,
                    Room = roomVM,
                    CurrentReservationClientCount = clientsCount,
                    DateOfAccommodation = reservation.DateOfAccommodation,
                    DateOfExemption = reservation.DateOfExemption,
                    IsAllInclusive = reservation.IsAllInclusive,
                    IsBreakfastIncluded = reservation.IsBreakfastIncluded,
                    OverallBill = reservation.OverallBill,
                });

            }

            model.Items = list;
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Reservations.CountAsync() / (double)PageSize);

            return View(model);
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            ReservationsCreateViewModel model = new ReservationsCreateViewModel();

            model = CreateReservationVMWithDropdown(model, null);

            return View(model);
        }

        // POST: Reservations/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationsCreateViewModel createModel)
        {
            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            if (ModelState.IsValid)
            {

                int daysDiff = CalculateDaysPassed(createModel.DateOfAccommodation, createModel.DateOfExemption);

                if (daysDiff <= 0)
                {
                    createModel = CreateReservationVMWithDropdown(createModel, "Date of accommodation must be before Date of exemption");
                    return View(createModel);
                }

                

                int userId = createModel.UserId;
                int roomId = createModel.RoomId;

                foreach (var item in _context.Reservations.Where(x => x.RoomId == roomId))
                {
                    if ((item.DateOfAccommodation > createModel.DateOfAccommodation && item.DateOfAccommodation < createModel.DateOfExemption)
                        ||
                        (item.DateOfExemption > createModel.DateOfAccommodation && item.DateOfExemption < createModel.DateOfExemption))
                    {
                        createModel = CreateReservationVMWithDropdown(createModel, $"Room is already reserved for the chosen period. Either choose a period before {item.DateOfAccommodation}, or after {item.DateOfExemption}");

                        return View(createModel);
                    }
                }

                Reservation reservation = new Reservation
                {
                    UserId = createModel.UserId,
                    RoomId = createModel.RoomId,
                    DateOfAccommodation = createModel.DateOfAccommodation,
                    DateOfExemption = createModel.DateOfExemption,
                    IsAllInclusive = createModel.IsAllInclusive,
                    IsBreakfastIncluded = createModel.IsBreakfastIncluded,
                    OverallBill = 0
                };

                _context.Add(reservation);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(createModel);
        }



        public async Task<IActionResult> Edit(int? id)
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            if (id == null)
            {
                return NotFound();
            }

            Reservation reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {

                return NotFound();
            }
            Room room = await _context.Rooms.FindAsync(reservation.RoomId);
            ReservationsEditViewModel model = new ReservationsEditViewModel()
            {
                Id = reservation.Id,
                DateOfAccommodation = reservation.DateOfAccommodation,
                DateOfExemption = reservation.DateOfExemption,
                IsAllInclusive = reservation.IsAllInclusive,
                IsBreakfastIncluded = reservation.IsBreakfastIncluded,
                OverallBill = reservation.OverallBill
            };

            return View(model);
        }

        // POST: Clients/Edit/5       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReservationsEditViewModel editModel)
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            int daysDiff = CalculateDaysPassed(editModel.DateOfAccommodation, editModel.DateOfExemption);

            if (daysDiff <= 0)
            {
                editModel.Message = "Date of accommodation must be before Date of exemption";
                return View(editModel);
            }

            int roomId = editModel.Id;

            foreach (var item in _context.Reservations.Where(x => x.RoomId == roomId))
            {
                if ((item.DateOfAccommodation > editModel.DateOfAccommodation && item.DateOfAccommodation < editModel.DateOfExemption)
                    ||
                    (item.DateOfExemption > editModel.DateOfAccommodation && item.DateOfExemption < editModel.DateOfExemption))
                {
                    editModel.Message =$"Room is already reserved for the chosen period. Either choose a period before {item.DateOfAccommodation}, or after {item.DateOfExemption}";

                    return View(editModel);
                }
            }

            if (ModelState.IsValid)
            {
                Reservation reservation = _context.Reservations.Find(editModel.Id);

                reservation.DateOfAccommodation = editModel.DateOfAccommodation;
                reservation.DateOfExemption = editModel.DateOfExemption;
                reservation.IsAllInclusive = editModel.IsAllInclusive;
                reservation.IsBreakfastIncluded = editModel.IsBreakfastIncluded;
               
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                reservation.OverallBill = CalculateOverAllPrice(reservation.Id);
                _context.Update(reservation);
                await _context.SaveChangesAsync();


                return RedirectToAction(nameof(Index));
            }

            return View(editModel);
        }


        // GET: Reservations/Detail
        public async Task<IActionResult> Detail(int? id)
        {
            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            if (id == null)
            {
                return NotFound();
            }

            Reservation reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }


            UsersViewModel userVM = new UsersViewModel()
            {
                Id = reservation.User.Id,
                FirstName = reservation.User.FirstName,
                MiddleName = reservation.User.MiddleName,
                LastName = reservation.User.LastName,
                Username = reservation.User.Username
            };

            RoomsViewModel roomVM = new RoomsViewModel()
            {
                Id = reservation.Room.Id,
                Capacity = reservation.Room.Capacity,
                PriceAdult = reservation.Room.PriceAdult,
                PriceChild = reservation.Room.PriceChild,
                Number = reservation.Room.Number,
                Type = (RoomTypeEnum)reservation.Room.Type
            };

            var allClients = await _context.Clients.ToListAsync();

            var allClientReservations = await _context.ClientReservation.Where(x => x.ReservationId == id).ToListAsync();

            var reservedClients = new List<Client>();

            var availableClients = allClients;

            foreach (var clientReservation in allClientReservations)
            {
                availableClients.RemoveAll(x => x.Id == clientReservation.ClientId);
                var client = (await _context.Clients.FindAsync(clientReservation.ClientId));
                reservedClients.Add(client);
            }

            var availableClientsVM = availableClients.Select(x => new SelectListItem()
            {
                Text = x.FirstName + " " + x.LastName + " (" + x.Email + ")",
                Value = x.Id.ToString()
            }).ToList();

            var reservedClientsVM = reservedClients.Select(x => new ClientsViewModel()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email

            }).ToList();

            var model = new ReservationsViewModel()
            {
                User = userVM,
                Room = roomVM,
                CurrentReservationClientCount = reservedClients.Count(),
                DateOfAccommodation = reservation.DateOfAccommodation,
                DateOfExemption = reservation.DateOfExemption,
                IsAllInclusive = reservation.IsAllInclusive,
                IsBreakfastIncluded = reservation.IsBreakfastIncluded,
                OverallBill = reservation.OverallBill,
                AvailableClients = availableClientsVM,
                SignedInClients = reservedClientsVM
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkClientReservation(ReservationsViewModel linkModel)
        {
            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            var clientId = linkModel.ClientId;
            var reservationId = linkModel.Id;

            if (reservationId <= 0)
            {
                return RedirectToAction("Index");
            }

            if (clientId <= 0)
            {
                return RedirectToAction("Detail", new { id = reservationId });
            }


            var clientReservation = new ClientReservation()
            {
                ClientId = clientId,
                ReservationId = reservationId
            };

            var currentRoomOccupyCount = (await _context.ClientReservation.Where(x => x.ReservationId == reservationId).ToListAsync()).Count;

            Room room = await _context.Rooms.FindAsync(_context.Reservations.Find(reservationId).RoomId);


            if (currentRoomOccupyCount >= room.Capacity)
            {
                return RedirectToAction("Detail", new { id = reservationId });
            }


            var elem = _context.ClientReservation.Find(clientId, reservationId);

            if (elem != null)
            {
                throw new ArgumentException($"CUSTOM EXCEPTION: This client {clientId} is already added to this reservation {reservationId}");
            }
            else
            {
                _context.ClientReservation.Add(clientReservation);
                
                await _context.SaveChangesAsync();

                bool isClientAdult = (await _context.Clients.FindAsync(clientId)).IsAdult;
                decimal pricePerDay = 0;
                if (isClientAdult)
                {
                    pricePerDay += (await _context.Rooms.FindAsync(room.Id)).PriceAdult;
                }
                else
                {
                    pricePerDay += (await _context.Rooms.FindAsync(room.Id)).PriceChild;
                }

                Reservation reservation = await _context.Reservations.FindAsync(reservationId);
                decimal clientOverall = pricePerDay*CalculateDaysPassed(reservation.DateOfAccommodation, reservation.DateOfExemption);
                clientOverall = AddExtras(clientOverall, reservation.IsAllInclusive, reservation.IsBreakfastIncluded);
                reservation.OverallBill += clientOverall;

                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Detail", new { id = reservationId });
            //return View("Detail",linkModel);
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
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

        private decimal CalculateOneDayPriceWithoutExtrasForRoom(int reservationId)
        {

            Reservation reservation = _context.Reservations.Find(reservationId);


            decimal underagePrice = _context.Rooms.Find(reservation.RoomId).PriceChild;
            decimal adultPrice = _context.Rooms.Find(reservation.RoomId).PriceAdult;

            var clientList = _context.ClientReservation.Where(x => x.ReservationId == reservationId).ToList();


            decimal pricePerDay = 0;
            foreach (var id in clientList)
            {
                bool isClientAdult = _context.Clients.Find(id.ClientId).IsAdult;
                if (isClientAdult)
                {
                    pricePerDay += adultPrice;
                }
                else
                {
                    pricePerDay += underagePrice;
                }
            }
            return pricePerDay;
        }

        private decimal CalculateOverallBillWithoutExtrasForRoom(int reservationId, int days)
        {
            return CalculateOneDayPriceWithoutExtrasForRoom(reservationId) * days;
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

        private decimal CalculateOverAllPrice(int reservationId)
        {
            Reservation reservation = _context.Reservations.Find(reservationId);
            int days = CalculateDaysPassed(reservation.DateOfAccommodation, reservation.DateOfExemption);
            var overallBillWithoutextras = CalculateOverallBillWithoutExtrasForRoom(reservationId, days);
            return AddExtras(overallBillWithoutextras, reservation.IsAllInclusive, reservation.IsBreakfastIncluded);
        }

        private ReservationsCreateViewModel CreateReservationVMWithDropdown(ReservationsCreateViewModel model, string message)
        {
            model.Message = message;

            model.Rooms = _context.Rooms.ToList().Select(x => new SelectListItem()
            {
                Text = $"{x.Number.ToString()} [0/{x.Capacity}] (type: {((RoomTypeEnum)x.Type).ToString()})",
                Value = x.Id.ToString()
            }).ToList();

            model.Users = _context.Users.Select(x => new SelectListItem()
            {
                Text = x.FirstName + " " + x.LastName + " (" + x.Email + ")",
                Value = x.Id.ToString()
            }).ToList();

            return model;
        }

    }
}
