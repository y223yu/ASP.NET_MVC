using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YSSail.Data;
using YSSail.Models;

namespace YSSail.Controllers
{
    [Authorize]
    //[Authorize(Roles = "Administrators")] //problem: fred cannot access even if he's an administrator
    //[AllowAnonymous] //just for test
    public class YYRolesController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext adb;

        public YYRolesController(
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

            List<IdentityRole> roles = roleManager.Roles.OrderBy(a => a.Name).ToList();
            return View(roles);
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            var authorization = userManager.GetUserAsync(HttpContext.User).Result;
            if (authorization != null)
            {
                TempData["isAdministraor"] = "Valid";
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(IdentityRole _roleName)
        {
            try
            {
                List<IdentityRole> roles = roleManager.Roles.OrderBy(a => a.Name).ToList();

                // if role exists, get role object & delete it
                if (await roleManager.RoleExistsAsync(_roleName.Name))
                {

                    TempData["role_error_message"] = "role already exists: " + _roleName;
                    return View("Index", roles);
                }

                // create role – just need a name to instantiate a role object
                IdentityResult identityResult = await roleManager.CreateAsync(_roleName);
                if (identityResult.Succeeded)
                {
                    TempData["role_error_message"] = "role created:" + _roleName.Name;
                    roles = roleManager.Roles.OrderBy(a => a.Name).ToList();
                    return View("Index", roles);
                }
                else
                {
                    // if Create failed without an exception, tell user & proceed to sad path
                    TempData["role_error_message"] = $"error creating role:{identityResult.Errors.FirstOrDefault().Description}";
                    return View("Index", roles);
                }
            }
            catch (Exception ex)
            {
                TempData["role_error_message"] = "exception deleting & creating role: " +
                        ex.GetBaseException().Message;
                return View("Index");
            }

        }

        // GET: Roles/Delete
        public IActionResult Delete(string id)
        {
            var authorization = userManager.GetUserAsync(HttpContext.User).Result;
            if (authorization != null)
            {
                TempData["isAdministraor"] = "Valid";
            }
            var result = new IdentityRole(id);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAsync(IdentityRole _roleName)
        {
            try
            {
                List<IdentityRole> roles = roleManager.Roles.OrderBy(a => a.Name).ToList();
                // if role exists, get role object & delete it

                if (_roleName.Name.ToLower().Equals("administrators"))
                {
                    TempData["role_error_message"] = "administrators role cannot be deleted:";
                    return View("Index", roles);
                }

                if (await roleManager.RoleExistsAsync(_roleName.Name))
                {
                    var role = await roleManager.FindByNameAsync(_roleName.Name);
                    IdentityResult identityResult = await roleManager.DeleteAsync(role);
                    if (identityResult.Succeeded)
                    {
                        TempData["role_error_message"] = "role deleted:" + _roleName.Name;
                        roles = roleManager.Roles.OrderBy(a => a.Name).ToList();
                        return View("Index", roles);
                    }
                    else
                    {
                        // if Create failed without an exception, tell user & proceed to sad path
                        TempData["role_error_message"] = $"error creating role:{identityResult.Errors.FirstOrDefault().Description}";
                        return View("Index", roles);
                    }
                }
                else
                {
                    TempData["role_error_message"] = "cannot find role to delete: " + _roleName.Name;
                    return View("Index", roles);
                }
            }
            catch (Exception ex)
            {
                TempData["role_error_message"] = "exception deleting role: " +
                        ex.GetBaseException().Message;
                return View("Index");
            }
        }

        // GET: Roles/Details
        public async Task<IActionResult> RoleDetails(string Id)
        {
            var authorization = userManager.GetUserAsync(HttpContext.User).Result;
            if (authorization != null)
            {
                TempData["isAdministraor"] = "Valid";
            }
            try
            {
                if (Id == null)
                {
                    throw new ArgumentNullException("RoleId cannot be null");
                }
                var _role = roleManager.FindByIdAsync(Id).Result;
                if (_role == null)
                {
                    throw new ArgumentNullException("Role cannot be null");
                }
                var _usersInRole = await userManager.GetUsersInRoleAsync(_role.Name);
                ViewData["RoleName"] = _role.Name;

                var usrlist = adb.UserRoles.Join(adb.Users, usr => usr.UserId, u => u.Id, (usr, u) => new { user = u, userRole = usr })
                    .Where(a => a.userRole.RoleId == _role.Id).Select(x => x.user.Id)
                    .Distinct().ToList();

                var results = adb.Users.Where(x => !usrlist.Any(i => x.Id == i)).OrderBy(s => s.UserName).ToList();

                ViewData["userList"] = new SelectList(results, "Id", "UserName");
                return View(_usersInRole);

            }
            catch (Exception ex)
            {
                TempData["role_error_message"] = "exception deleting role: " +
                        ex.GetBaseException().Message;
                return View("Index");
            }

        }

        [HttpGet]
        public async Task<IActionResult> RemoveUserFromRole(string Id, string roleName)
        {
            try
            {
                if (roleName == null || Id == null)
                {
                    throw new ArgumentNullException("Role or userId cannot be null");
                }

                var _user = userManager.FindByIdAsync(Id).Result;
                if (_user == null)
                {
                    throw new NullReferenceException("User to remove from role cannot be found");
                }

                IdentityResult result = await userManager.RemoveFromRoleAsync(_user, roleName);

                if (result.Succeeded)
                {
                    var _role = await roleManager.FindByNameAsync(roleName);
                    return RedirectToAction("RoleDetails", "YYRoles", new { id = _role.Id });
                }
                else
                {
                    TempData["role_error_message"] = "error adding user to role";
                    return View("Index");
                }

                //var _usersInRole = await userManager.GetUsersInRoleAsync(roleName);
                //return View("RoleDetails", _usersInRole);

            }
            catch (Exception ex)
            {
                TempData["role_error_message"] = "exception deleting role: " +
                        ex.GetBaseException().Message;
                return View("Index");
            }

        }

        [HttpGet]
        public async Task<IActionResult> AddUserToRole(string roleName, string Id)
        {
            try
            {
                if (roleName == null || Id == null)
                {
                    throw new ArgumentNullException("Role or userId cannot be null");
                }

                var _user = userManager.FindByIdAsync(Id).Result;
                if (_user == null)
                {
                    throw new NullReferenceException("User to remove from role cannot be found");
                }

                Task<IdentityResult> result = userManager.AddToRoleAsync(_user, roleName);

                if (result.Result.Succeeded)
                {
                    var _role = await roleManager.FindByNameAsync(roleName);
                    return RedirectToAction("RoleDetails", "YYRoles", new { id = _role.Id });
                }
                else
                {
                    TempData["role_error_message"] = "error adding user to role";
                    return View("Index");
                }

            }
            catch (Exception ex)
            {
                TempData["role_error_message"] = "exception deleting role: " +
                        ex.GetBaseException().Message;
                return View("Index");
            }
        }
    }
}