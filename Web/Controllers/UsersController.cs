using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Entity;
using Web.Models.Users;
using Web.Models.Shared;
using Web.Models;
using Web.Models.Reservations;

namespace Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly int PageSize = GlobalVar.AmountOfElementsDisplayedPerPage;

        private readonly HotelReservationDb _context;

        public UsersController()
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

        /*
        public IActionResult ChangePageSize(PageCountChangeViewModel PageProperties)
        {

            if (PageProperties.PagesCount > 0)
            {
                PageSize = PageProperties.PagesCount;
            }

            return RedirectToAction("Index");
        }*/

        // GET: Users
        public async Task<IActionResult> Index(UsersIndexViewModel model)
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            var allUsers = await _context.Users.Where(x => x.IsActive).ToListAsync();

            var contextDb = Filter(allUsers, model.Filter);

            List<UsersViewModel> items = contextDb.Skip((model.Pager.CurrentPage - 1) * this.PageSize).Take(this.PageSize).Select(c => new UsersViewModel()
            {
                Id = c.Id,
                Username = c.Username,
                Password = c.Password,
                FirstName = c.FirstName,
                MiddleName = c.MiddleName,
                LastName = c.LastName,
                EGN = c.EGN,
                Email = c.Email,
                TelephoneNumber = c.TelephoneNumber,
                DateOfBeingFired = c.DateOfBeingFired,
                DateOfBeingHired = c.DateOfBeingHired

            }).ToList();

            model.Items = items;
            model.Pager.PagesCount = (int)Math.Ceiling(await _context.Users.CountAsync() / (double)this.PageSize);

            return View(model);
        }

        // GET: Users/Create
        public IActionResult Create()
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.admininstration)
            {
                UsersLogInViewModel model1 = new UsersLogInViewModel();
                model1.Message = "You dont meet the required permission to do this. Please, log in into account with admin permissions";
                return View("LogIn", model1);
            }

            UsersCreateViewModel model = new UsersCreateViewModel();

            return View(model);
        }

        // POST: Users/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsersCreateViewModel createModel)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.admininstration)
            {
                UsersLogInViewModel model1 = new UsersLogInViewModel();
                model1.Message = "You dont meet the required permission to do this. Please, log in into account with admin permissions";
                return View("LogIn", model1);
            }

            if (!DoesPasswordsMatch(createModel.Password, createModel.ConfirmPassword))
            {
                createModel.Message = "Password and confirm password should match";
                return View(createModel);
            }

            if (_context.Users.Where(x => x.Username == createModel.Username).Count() > 0)
            {
                createModel.Message = $"User cant be created becuase there's already an existing user with the given username ({createModel.Username})";
                return View(createModel);
            }

            if (!createModel.IsAdult)
            {
                createModel.Message = "The checkbox is required";
                return View(createModel);
            }

            createModel.Message = null;
            if (ModelState.IsValid)
            {
                User user = new User
                {
                    Username = createModel.Username,
                    Password = createModel.Password,
                    FirstName = createModel.FirstName,
                    MiddleName = createModel.MiddleName,
                    LastName = createModel.LastName,
                    EGN = createModel.EGN,
                    Email = createModel.Email,
                    TelephoneNumber = createModel.TelephoneNumber
                };

                _context.Add(user);
                await _context.SaveChangesAsync();

                Client client = new Client
                {
                    FirstName = createModel.FirstName,
                    LastName = createModel.LastName,
                    Email = createModel.Email,
                    TelephoneNumber = createModel.TelephoneNumber,
                    IsAdult = true
                };

                _context.Add(client);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(createModel);
        }

        public IActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogIn(UsersLogInViewModel model)
        {

            User user = _context.Users.Where(x => x.Username == model.Username).FirstOrDefault();

            if (user == null || (user.Password != model.Password))
            {

                UsersLogInViewModel model1 = new UsersLogInViewModel();
                model1.Message = "Username and password combination doesnt match";
                return View(model1);


            }

            GlobalVar.LoggedOnUserId = user.Id;
            if (user.Id == 1)
            {
                GlobalVar.LoggedOnUserRights = GlobalVar.UserRights.admininstration;
            }
            else
            {
                GlobalVar.LoggedOnUserRights = GlobalVar.UserRights.defaultUser;
            }

            return RedirectToAction("Index", "Users");

        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.admininstration)
            {
                UsersLogInViewModel model1 = new UsersLogInViewModel();
                model1.Message = "You dont meet the required permission to do this. Please, log in into account with admin permissions";
                return View("LogIn", model1);
            }

            if (id == null)
            {
                return NotFound();
            }

            User user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            UsersEditViewModel model = new UsersEditViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Password = user.Password,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                EGN = user.EGN,
                Email = user.Email,
                TelephoneNumber = user.TelephoneNumber
            };

            return View(model);
        }

        // POST: Users/Edit/5       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsersEditViewModel editModel)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.admininstration)
            {
                UsersLogInViewModel model1 = new UsersLogInViewModel();
                model1.Message = "You dont meet the required permission to do this. Please, log in into account with admin permissions";
                return View("LogIn", model1);
            }

            if (ModelState.IsValid)
            {

                User user = new User()
                {
                    Id = editModel.Id,
                    Username = editModel.Username,
                    Password = editModel.Password,
                    FirstName = editModel.FirstName,
                    MiddleName = editModel.MiddleName,
                    LastName = editModel.LastName,
                    EGN = editModel.EGN,
                    Email = editModel.Email,
                    TelephoneNumber = editModel.TelephoneNumber,

                };

                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.admininstration)
            {
                UsersLogInViewModel model1 = new UsersLogInViewModel();
                model1.Message = "You dont meet the required permission to do this. Please, log in into account with admin permissions";
                return View("LogIn", model1);
            }

            if (id == 1)
            {
                throw new ArgumentException("----------------------------------------------\nPlease, do not erase yourself----------------------------------------------");
            }

            User user = await _context.Users.FindAsync(id);
            user.DateOfBeingFired = DateTime.UtcNow;
            user.IsActive = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult LogInRequired()
        {
            return View();
        }
        public IActionResult LogInPermissionDenied()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LogInRequired(UsersLogInViewModel model)
        {
            return LogIn(model);
        }

        [HttpPost]
        public IActionResult LogInPermissionDenied(UsersLogInViewModel model)
        {
            return LogIn(model);
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
        private bool DoesPasswordsMatch(string password, string confirmPassword)
        {
            return password == confirmPassword;
        }

        private List<User> Filter(List<User> collection, UsersFilterViewModel filterModel)
        {

            if (filterModel != null)
            {
                if (filterModel.Username != null)
                {
                    collection = collection.Where(x => x.Username.Contains(filterModel.Username)).ToList();
                }
                if (filterModel.FirstName != null)
                {
                    collection = collection.Where(x => x.FirstName.Contains(filterModel.FirstName)).ToList();
                }
                if (filterModel.MiddleName != null)
                {
                    collection = collection.Where(x => x.MiddleName.Contains(filterModel.MiddleName)).ToList();
                }
                if (filterModel.LastName != null)
                {
                    collection = collection.Where(x => x.LastName.Contains(filterModel.LastName)).ToList();
                }
                if (filterModel.Email != null)
                {
                    collection = collection.Where(x => x.Email.Contains(filterModel.Email)).ToList();
                }
            }

            return collection;
        }
    }
}
