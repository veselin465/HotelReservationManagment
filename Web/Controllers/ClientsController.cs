using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Entity;
using Web.Models.Clients;
using Web.Models.Shared;
using Web.Models.Reservations;
using Web.Models.Rooms;
using Data.Enumeration;

namespace Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly int PageSize = GlobalVar.AmountOfElementsDisplayedPerPage;
        private readonly HotelReservationDb _context;

        public ClientsController()
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

        // GET: Clients
        public IActionResult Index(ClientsIndexViewModel model)
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            var contextDb = Filter( _context.Clients.ToList(), model.Filter);

            List<ClientsViewModel> items = contextDb.Skip((model.Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(c => new ClientsViewModel()
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                TelephoneNumber = c.TelephoneNumber,
                IsAdult = c.IsAdult
            }).ToList();

            if (model.Filter == null)
            {
                model.Filter = new ClientsFilterViewModel();
            }
            model.Items = items;
            model.Pager.PagesCount = Math.Max(1, (int)Math.Ceiling(contextDb.Count() / (double)PageSize));

            return View(model);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            ClientsCreateViewModel model = new ClientsCreateViewModel();

            return View(model);
        }

        // POST: Clients/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ClientsCreateViewModel createModel)
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            if (ModelState.IsValid)
            {
                Client client = new Client
                {
                    FirstName = createModel.FirstName,
                    LastName = createModel.LastName,
                    Email = createModel.Email,
                    TelephoneNumber = createModel.TelephoneNumber,
                    IsAdult = createModel.IsAdult
                };

                _context.Clients.Add(client);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // GET: Clients/Edit/5
        public IActionResult Edit(int? id)
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            if (id == null)
            {
                return NotFound();
            }

            Client client =  _context.Clients.Find(id);
            if (client == null)
            {

                return NotFound();
            }

            ClientsEditViewModel model = new ClientsEditViewModel
            {
                Id = client.Id,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                TelephoneNumber = client.TelephoneNumber,
                IsAdult = client.IsAdult
            };

            return View(model);
        }

        // POST: Clients/Edit/5       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ClientsEditViewModel editModel)
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            if (ModelState.IsValid)
            {
                Client client = new Client()
                {
                    Id = editModel.Id,
                    FirstName = editModel.FirstName,
                    LastName = editModel.LastName,
                    Email = editModel.Email,
                    TelephoneNumber = editModel.TelephoneNumber,
                    IsAdult = editModel.IsAdult
                };

                try
                {
                    _context.Update(client);
                     _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(editModel);
        }



        public IActionResult Detail(int? id)
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            if (id == null||!ClientExists((int)id))
            {
                return NotFound();
            }

            Client client =  _context.Clients.Find(id);
            List<Reservation> reservations =  _context.Reservations.ToList();
            List<ClientReservation> clientReservations =  _context.ClientReservation.Where(x => x.ClientId == id).ToList();

            foreach (var cr in clientReservations)
            {
                reservations.RemoveAll(x => x.Id == cr.ReservationId);
            }

            ClientsDetailViewModel model = new ClientsDetailViewModel()
            {
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                IsAdult = client.IsAdult,
                PastReservations = reservations.Where(x => x.DateOfExemption.AddHours(GlobalVar.DefaultReservationHourStart) < DateTime.UtcNow).Select(x=>new ReservationsViewModel()
                {
                    Id = x.Id,
                    DateOfAccommodation = x.DateOfAccommodation.AddHours(GlobalVar.DefaultReservationHourStart),
                    DateOfExemption = x.DateOfExemption.AddHours(GlobalVar.DefaultReservationHourStart),
                    IsBreakfastIncluded = x.IsBreakfastIncluded,
                    IsAllInclusive = x.IsAllInclusive,
                    OverallBill = x.OverallBill,
                    Room = new RoomsViewModel()
                    {
                        Number = _context.Rooms.Find(x.RoomId).Number,
                        Capacity = _context.Rooms.Find(x.RoomId).Capacity,
                        Type = (RoomTypeEnum)_context.Rooms.Find(x.RoomId).Type
                    }
                }).ToList(),
                UpcomingReservations = reservations.Where(x => x.DateOfExemption.AddHours(GlobalVar.DefaultReservationHourStart) >= DateTime.UtcNow).Select(x => new ReservationsViewModel()
                {
                    Id = x.Id,
                    DateOfAccommodation = x.DateOfAccommodation.AddHours(GlobalVar.DefaultReservationHourStart),
                    DateOfExemption = x.DateOfExemption.AddHours(GlobalVar.DefaultReservationHourStart),
                    IsBreakfastIncluded = x.IsBreakfastIncluded,
                    IsAllInclusive = x.IsAllInclusive,
                    OverallBill = x.OverallBill,
                    Room = new RoomsViewModel()
                    {
                        Number = _context.Rooms.Find(x.RoomId).Number,
                        Capacity = _context.Rooms.Find(x.RoomId).Capacity,
                        Type = (RoomTypeEnum)_context.Rooms.Find(x.RoomId).Type
                    }
                }).ToList(),
                TelephoneNumber = client.TelephoneNumber
                
            };

            

            return View(model);
        }




        // GET: Clients/Delete/5
        public IActionResult Delete(int id)
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            Client client =  _context.Clients.Find(id);
            _context.Clients.Remove(client);
             _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
        private List<Client> Filter(List<Client> collection, ClientsFilterViewModel filterModel)
        {

            if (filterModel != null)
            {
                if (filterModel.FirstName != null)
                {
                    collection = collection.Where(x => x.FirstName.Contains(filterModel.FirstName)).ToList();
                }
                if (filterModel.LastName != null)
                {
                    collection = collection.Where(x => x.LastName.Contains(filterModel.LastName)).ToList();
                }
            }

            return collection;
        }

       

    }
}
