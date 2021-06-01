using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthPracticeLibrary;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserHasMongoStoredAccount.Models;

namespace UserHasMongoStoredAccount.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MongoCRUD _db;

        public HomeController(ILogger<HomeController> logger, MongoCRUD db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        // POST: Home/Index (validates login)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string username, string password)
        {
            try
            {
                PersonModel person = _db.LoadRecords<PersonModel>("Users")
                    .Where(p => p.FirstName == username).First();

                string[] splitHash = person.PasswordHash.Split('.');
                int iterations = int.Parse(splitHash[0]);
                (bool IsPasswordCorrect, bool needsUpgrade) = HashAndSalter.PasswordEqualsHash(password, splitHash[2], splitHash[1], iterations);

                if (IsPasswordCorrect)
                {

                    List<Claim> personClaims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, person.FirstName),
                        new Claim(ClaimTypes.Role, person.PersonRole)
                    };

                    List<ClaimsIdentity> claimsIdentities = new List<ClaimsIdentity>()
                    {
                        new ClaimsIdentity(personClaims, "Hi more test strings")
                    };

                    ClaimsPrincipal multiClaimIdentityContainerThing = new ClaimsPrincipal(claimsIdentities);

                    HttpContext.SignInAsync(multiClaimIdentityContainerThing);

                    return RedirectToAction(nameof(GuestPage));
                } 
                else
                {
                    throw new FormatException("Incorrect password hash storage format");
                }
            }
            catch
            {
                return View();
            }
        }

        [Authorize(Policy="Test Policy")]
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult GuestPage()
        {
            return View();
        }

        public IActionResult AccessDenied()
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
