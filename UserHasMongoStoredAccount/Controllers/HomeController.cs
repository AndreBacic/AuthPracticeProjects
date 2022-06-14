using AuthPracticeLibrary;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using UserHasMongoStoredAccount.Models;

namespace UserHasMongoStoredAccount.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MongoCRUD _db;
        private readonly IHashAndSalter _hashAndSalter;

        public HomeController(ILogger<HomeController> logger, 
                              MongoCRUD db, 
                              IHashAndSalter hashAndSalter)
        {
            _logger = logger;
            _db = db;
            _hashAndSalter = hashAndSalter;
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
            PersonModel person;
            try
            {
                person = _db.LoadRecords<PersonModel>("Users")
                    .Where(p => p.FirstName == username).First();
            }
            catch
            {
                return View();
            }
            string[] splitHash = person.PasswordHash.Split('.');
            int iterations = int.Parse(splitHash[0]);
            (bool IsPasswordCorrect, bool needsUpgrade) = _hashAndSalter.PasswordEqualsHash(password, splitHash[2], splitHash[1], iterations);

            if (IsPasswordCorrect == false)
            {
                return View();
            }

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

        [Authorize(Policy = "Test Policy")]
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
