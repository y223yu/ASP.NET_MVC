using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YSSail.ModelView;

namespace YSSail.Controllers
{
    [Authorize]
    public class YYUsersController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public YYUsersController(
      UserManager<IdentityUser> userManager,
      RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var authorization = userManager.GetUserAsync(HttpContext.User).Result;
            if (authorization != null)
            {
                TempData["isAdministraor"] = "Valid";
            }

            List<IdentityUser> users = userManager.Users.OrderBy(a => a.UserName).ToList();
            List<SailUser> result = new List<SailUser>();

            users.ForEach(usr => {
                SailUser _suser = new SailUser();
                _suser.user = usr;
                var roles = userManager.GetRolesAsync(usr).Result;
                if (roles.Any(x => x.Contains("members")))
                    _suser.isMember = true;
                if (roles.Any(x => x.Contains("staff")))
                    _suser.isStaff = true;

                if (roles.Any(x => x.Contains("administrators")))
                    _suser.isAdministrator = true;

                result.Add(_suser);
            });
            return View(result.OrderBy(x => x.isMember == false).ThenBy(x => x.user.UserName).ToList());

        }

        public IActionResult LockedOutFilter()
        {
            List<IdentityUser> users = userManager.Users.OrderBy(a => a.UserName).ToList();
            List<SailUser> result = new List<SailUser>();

            users.ForEach(usr => {
                SailUser _suser = new SailUser();
                _suser.user = usr;
                var roles = userManager.GetRolesAsync(usr).Result;
                if (roles.Any(x => x.Contains("members")))
                    _suser.isMember = true;
                if (roles.Any(x => x.Contains("administrators")))
                    _suser.isAdministrator = true;

                result.Add(_suser);
            });
            return View("Index", result.OrderBy(x => x.user.LockoutEnd.HasValue).ThenBy(x => x.user.UserName).ToList());

        }

        public async Task<IActionResult> LockUser(string Id)
        {
            try
            {
                var user = userManager.FindByIdAsync(Id).Result;
                if (user == null)
                    throw new NullReferenceException("cannot find user to lock out");

                IdentityResult identityResult = await userManager.SetLockoutEndDateAsync(user, DateTime.Now.AddDays(7));
                if (identityResult.Succeeded)
                {
                    TempData["user_error_message"] = "user locked out:" + user.UserName;

                    return RedirectToAction("Index");
                }
                else
                {
                    // if Create failed without an exception, tell user & proceed to sad path
                    TempData["user_error_message"] = $"error locking user:{identityResult.Errors.FirstOrDefault().Description}";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["user_error_message"] = "exception deleting & creating role: " +
                        ex.GetBaseException().Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> UnlockUser(string Id)
        {
            try
            {
                var user = userManager.FindByIdAsync(Id).Result;
                if (user == null)
                    throw new NullReferenceException("cannot find user to lock out");

                IdentityResult identityResult = await userManager.SetLockoutEndDateAsync(user, null);
                if (identityResult.Succeeded)
                {
                    TempData["user_error_message"] = "user locked out:" + user.UserName;

                    return RedirectToAction("Index");
                }
                else
                {
                    // if Create failed without an exception, tell user & proceed to sad path
                    TempData["user_error_message"] = $"error locking user:{identityResult.Errors.FirstOrDefault().Description}";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["user_error_message"] = "exception deleting & creating role: " +
                        ex.GetBaseException().Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string Id, string currentPassword, string newPassword)
        {
            try
            {
                var user = userManager.FindByIdAsync(Id).Result;
                if (user == null)
                    throw new NullReferenceException("cannot find user to lock out");


                // create role – just need a name to instantiate a role object
                IdentityResult identityResult = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

                if (identityResult.Succeeded)
                {
                    TempData["user_error_message"] = "user locked out:" + user.UserName;

                    return RedirectToAction("Index");
                }
                else
                {
                    // if Create failed without an exception, tell user & proceed to sad path
                    TempData["user_error_message"] = $"error locking user:{identityResult.Errors.FirstOrDefault().Description}";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["user_error_message"] = "exception deleting & creating role: " +
                        ex.GetBaseException().Message;
                return RedirectToAction("Index");
            }
        }


        public async Task<IActionResult> AddToMemberRole(string Id)
        {
            try
            {
                var user = userManager.FindByIdAsync(Id).Result;
                if (user == null)
                    throw new NullReferenceException("cannot find user to lock out");

                if (!roleManager.RoleExistsAsync("members").Result)
                    throw new NullReferenceException("cannot find members role to add");

                IdentityResult identityResult = await userManager.AddToRoleAsync(user, "members");

                if (identityResult.Succeeded)
                {
                    TempData["user_error_message"] = "user locked out:" + user.UserName;

                    return RedirectToAction("Index");
                }
                else
                {
                    // if Create failed without an exception, tell user & proceed to sad path
                    TempData["user_error_message"] = $"error locking user:{identityResult.Errors.FirstOrDefault().Description}";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["user_error_message"] = "exception deleting & creating role: " +
                        ex.GetBaseException().Message;
                return RedirectToAction("Index");
            }
        }
    }
}