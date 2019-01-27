using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YSSail.Data;
using YSSail.Models;

namespace YSSail.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext adb;

        public HomeController(
      UserManager<IdentityUser> userManager,
      RoleManager<IdentityRole> roleManager,
      ApplicationDbContext _adb)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.adb = _adb;
        }
        public IActionResult Index()
        {
            var authorization = userManager.GetUserAsync(HttpContext.User).Result;
            if (authorization != null)
            {
                TempData["isAdministraor"] = "Valid";
            }
            TempData["message"] = "Before.... But it is Yilong's website now";
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [Authorize(Roles = "members")]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
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
