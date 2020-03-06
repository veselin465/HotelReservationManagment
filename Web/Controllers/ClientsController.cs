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
        public async Task<IActionResult> Index(ClientsIndexViewModel model)
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            var contextDb = Filter(await _context.Clients.ToListAsync(), model.Filter);

            List<ClientsViewModel> items = contextDb.Skip((model.Pager.CurrentPage - 1) * PageSize).Take(PageSize).Select(c => new ClientsViewModel()
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                TelephoneNumber = c.TelephoneNumber,
                IsAdult = c.IsAdult
            }).ToList();

            model.Items = items;
            model.Filter = new ClientsFilterViewModel();
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Clients.CountAsync() / (double)PageSize);

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
        public async Task<IActionResult> Create(ClientsCreateViewModel createModel)
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

                _context.Add(client);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(createModel);
        }

        // GET: Clients/Edit/5
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

            Client client = await _context.Clients.FindAsync(id);
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
        public async Task<IActionResult> Edit(ClientsEditViewModel editModel)
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
                    await _context.SaveChangesAsync();
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

            Client client = await _context.Clients.FindAsync(id);
            List<Reservation> reservations = await _context.Reservations.ToListAsync();
            List<ClientReservation> clientReservations = await _context.ClientReservation.Where(x => x.ClientId == id).ToListAsync();

            foreach (var cr in clientReservations)
            {
                reservations.RemoveAll(x => x.Id == cr.ReservationId);
            }


            ClientsDetailViewModel x = new ClientsDetailViewModel()
            {
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                IsAdult = client.IsAdult,
                PastReservations = reservations.Where(x=>x.DateOfExemption<DateTime.UtcNow).Select(x=>new ReservationsViewModel()
                {
                    Id = x.Id,
                    DateOfAccommodation = x.DateOfAccommodation,
                    DateOfExemption = x.DateOfExemption,
                    IsAllInclusive = x.IsAllInclusive,
                    OverallBill = x.OverallBill
                }).ToList()
                
            };

            

            return View();
        }




        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int id)
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            Client client = await _context.Clients.FindAsync(id);
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

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
