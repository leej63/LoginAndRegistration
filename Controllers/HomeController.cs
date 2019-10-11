using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LoginAndRegistration.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace LoginAndRegistration.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        // Register View Page
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("Index");
        }
        
        // Register User in DB
        [HttpPost("")]
        public IActionResult RegisterUser(User newUser)
        {
            if(ModelState.IsValid)
            {
                // If a User exists with provided email
                if(dbContext.Users.Any(u => u.Email == newUser.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided error message
                    ModelState.AddModelError("Email", "Email is already in use!");
                    return View("Index"); // here or outside statement??
                }
                // hash password
                // Initializing a PasswordHasher object, providing our User class as its
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                // save to DB
                dbContext.Add(newUser);
                dbContext.SaveChanges();
                // Log New User in directly after successful registration
                HttpContext.Session.SetInt32("LoggedIn", 1);
                return RedirectToAction("SuccessPage");
            }
            return View("Index");
        }

        // Login View Page
        [HttpGet("login")]
        public IActionResult LoginPage()
        {
            return View("Login");
        }

        // Validates login info is correct
        [HttpPost("login")]
        public IActionResult LoginUser(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                // If initial ModelState is valid, query for a user with provided email
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                // If no user exists with provided email
                if(userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();
                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                // result can be compared to 0 for failure
                if(result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ModelState.AddModelError("Password", "Password is incorrect, please try again.");
                    return View("Login");
                }
                // Put User in session if everything is valid
                HttpContext.Session.SetInt32("LoggedIn", 1);
                return RedirectToAction("SuccessPage");
            }
            return View("Login");
        }

        // Success page - only logged-in users should be able to see this page
        [HttpGet("success")]
        public IActionResult SuccessPage()
        {
            int? LoggedUser = HttpContext.Session.GetInt32("LoggedIn");
            if(LoggedUser == 1)
            {
                return View("Success");
            }
            return RedirectToAction("LoginPage");
        }

        [HttpGet("logout")]
        public IActionResult LogoutUser()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("LoginPage");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
