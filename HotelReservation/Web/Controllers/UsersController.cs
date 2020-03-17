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
using Web.Models.Validation;

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

        // GET: Users
        public IActionResult Index(UsersIndexViewModel model)
        {

            if (GlobalVar.LoggedOnUserId == -1)
            {
                return RedirectToAction("LogInRequired", "Users");
            }

            model.Pager ??= new PagerViewModel();
            model.Pager.CurrentPage = model.Pager.CurrentPage <= 0 ? 1 : model.Pager.CurrentPage;

            var allUsers = _context.Users.ToList();

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
            model.Pager.PagesCount = Math.Max(1, (int)Math.Ceiling(contextDb.Count() / (double)this.PageSize));

            return View(model);
        }

        // GET: Users/Create
        public IActionResult Create()
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.Admininstrator && _context.Users.Where(x => x.IsActive).Count() != 0)
            {
                UsersLogInViewModel model1 = new UsersLogInViewModel();
                model1.Message = "You dont meet the required permission to do this. Please, log in into account with admin permissions";
                return View("LogIn", model1);
            }

            //UsersCreateViewModel model = new UsersCreateViewModel();

            return View();
        }

        // POST: Users/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UsersCreateViewModel createModel)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.Admininstrator && _context.Users.Where(x => x.IsActive).Count() != 0)
            {
                UsersLogInViewModel model1 = new UsersLogInViewModel();
                model1.Message = "You dont meet the required permission to do this. Please, log in into account with admin permissions";
                return View("LogIn", model1);
            }

            createModel.Message = null;
            if (ModelState.IsValid)
            {

                if (!DoesPasswordsMatch(createModel.Password, createModel.ConfirmPassword))
                {
                    createModel.Message = "Password and confirm password should match";
                    return View(createModel);
                }

                try
                {
                    Validate(new Validation_User()
                    {
                        Username = createModel.Username,
                        UserId = -1
                    });
                }
                catch (InvalidOperationException e)
                {
                    createModel.Message = e.Message;
                    return View(createModel);
                }

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

                _context.Users.Add(user);
                _context.SaveChanges();

                if (_context.Users.Where(x => x.IsActive).Count() == 1)
                {
                    GlobalVar.LoggedOnUserId = _context.Users.Where(x => x.IsActive).First().Id;
                    GlobalVar.LoggedOnUserRights = GlobalVar.UserRights.Admininstrator;
                }

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
                UsersLogInViewModel model1 = new UsersLogInViewModel
                {
                    Message = "Username and password combination doesnt match"
                };
                return View(model1);
            }

            if (!user.IsActive)
            {
                UsersLogInViewModel model1 = new UsersLogInViewModel
                {
                    Message = "This user is no longer active and therefore you cannot use it."
                };
                return View(model1);
            }

            GlobalVar.LoggedOnUserId = user.Id;
            if (user.Id == _context.Users.Where(x => x.IsActive).First().Id)
            {
                GlobalVar.LoggedOnUserRights = GlobalVar.UserRights.Admininstrator;
            }
            else
            {
                GlobalVar.LoggedOnUserRights = GlobalVar.UserRights.DefaultUser;
            }

            return RedirectToAction("Index", "Users");

        }

        // GET: Users/Edit/5
        public IActionResult Edit(int? id)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.Admininstrator)
            {
                UsersLogInViewModel model1 = new UsersLogInViewModel();
                model1.Message = "You dont meet the required permission to do this. Please, log in into account with admin permissions";
                return View("LogIn", model1);
            }

            if (id == null || !UserExists((int)id))
            {
                return NotFound();
            }

            User user = _context.Users.Find(id);

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
                TelephoneNumber = user.TelephoneNumber,
                IsActive = user.IsActive,
                FiredOn = user.DateOfBeingFired
            };

            return View(model);
        }

        // POST: Users/Edit/5       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UsersEditViewModel editModel)
        {

            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.Admininstrator)
            {
                UsersLogInViewModel model1 = new UsersLogInViewModel();
                model1.Message = "You dont meet the required permission to do this. Please, log in into account with admin permissions";
                return View("LogIn", model1);
            }

            if (ModelState.IsValid)
            {

                if (!UserExists(editModel.Id))
                {
                    return NotFound();
                }

                try
                {
                    Validate(new Validation_User()
                    {
                        Username = editModel.Username,
                        UserId = editModel.Id
                    });
                }
                catch (InvalidOperationException e)
                {
                    editModel.Message = e.Message;
                    return View(editModel);
                }

                User user = _context.Users.Find(editModel.Id);

                user.Username = editModel.Username;
                user.FirstName = editModel.FirstName;
                user.MiddleName = editModel.MiddleName;
                user.LastName = editModel.LastName;
                user.EGN = editModel.EGN;
                user.Email = editModel.Email;
                user.TelephoneNumber = editModel.TelephoneNumber;

                if (!String.IsNullOrEmpty(editModel.Password))
                {
                    user.Password = editModel.Password;
                }

                _context.Update(user);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(editModel);

        }

        // GET: Users/Delete/5
        public IActionResult Delete(int? id)
        {
            if (GlobalVar.LoggedOnUserRights != GlobalVar.UserRights.Admininstrator)
            {
                UsersLogInViewModel model1 = new UsersLogInViewModel();
                model1.Message = "You dont meet the required permission to do this. Please, log in into account with admin permissions";
                return View("LogIn", model1);
            }

            if (id == null || !UserExists((int)id))
            {
                return NotFound();
            }

            User user = _context.Users.Find(id);
            user.DateOfBeingFired = DateTime.UtcNow;
            user.IsActive = false;

            _context.Users.Update(user);
            _context.SaveChanges();

            if (user.Id == GlobalVar.LoggedOnUserId)
            {
                GlobalVar.LoggedOnUserId = -1;
                GlobalVar.LoggedOnUserRights = GlobalVar.UserRights.DefaultUser;
            }

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

        private void Validate(Validation_User model)
        {
         
            if (_context.Users.Where(x => x.Username == model.Username&&x.Id != model.UserId).Count() > 0)
            {
                throw new InvalidOperationException($"User cant be created becuase there's already an existing user with the given username ({model.Username})");
            }
        }
    }
}
