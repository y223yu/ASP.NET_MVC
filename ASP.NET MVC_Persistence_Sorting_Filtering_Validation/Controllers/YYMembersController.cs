using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YYSail.Models;
using System.Text.RegularExpressions;

namespace YYSail.Controllers
{
    public class YYMembersController : Controller
    {
        private readonly SailContext _context;

        public YYMembersController(SailContext context)
        {
            _context = context;
        }

        //check if ProvinceCode is fetched from database
        public JsonResult ProvinceCodeInDatabase(string provinceCode)
        {
            provinceCode = provinceCode.Trim().ToUpper();

            /*Method 1: using Find() keyword*/
            var provinces = _context.Province.Find(provinceCode);
            if (provinces == null)
                return Json("The province code is not valid.");
            else
                return Json(true);

            /*Method 2: using ToList() keyword*/
            //var provinces = _context.Province.Include(p => p.CountryCodeNavigation).ToList();
            //for (int i = 0; i < provinces.Count; i++)
            //{
            //    if (provinceCode == provinces[i].ProvinceCode)
            //    {
            //        return Json(true);
            //    }
            //}
            //return Json("The province code is not valid.");
        }

        // GET: YYMembers
        public async Task<IActionResult> Index()
        {
            var sailContext = _context.Member.Include(m => m.ProvinceCodeNavigation)
                .OrderBy(m => m.FullName);
            return View(await sailContext.ToListAsync());
        }

        // GET: YYMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id.HasValue)
            {
                var _member = _context.Member.Where(m => m.MemberId == id).SingleOrDefault();
                ViewData["FullName"] = _member.FullName;
            }

            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
                .Include(m => m.ProvinceCodeNavigation)
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // GET: YYMembers/Create
        public IActionResult Create()
        {
            //ViewData["ProvinceCode"] = new SelectList(_context.Province, "ProvinceCode", "ProvinceCode");
            Member _member = _context.Member.OrderByDescending(m => m.MemberId).FirstOrDefault();
            ViewData["memberId"] = _member.MemberId + 1;
            return View();
        }

        // POST: YYMembers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MemberId,FullName,FirstName,LastName,SpouseFirstName,SpouseLastName,Street,City,ProvinceCode,PostalCode,HomePhone,Email,YearJoined,Comment,TaskExempt,UseCanadaPost")] Member member)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var _member = _context.Member.OrderByDescending(m => m.MemberId).FirstOrDefault();
                    ViewData["memberId"] = member.MemberId + 1;
                    //member.MemberId = _member.MemberId + 1;
                    
                    _context.Add(member);
                    await _context.SaveChangesAsync();

                    TempData["CreateMessage"] = $"Record added: {member.FullName}";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"error creating new record: " + $" {ex.GetBaseException().Message}");
            }
            ViewData["ProvinceCode"] = new SelectList(_context.Province, "ProvinceCode", "Name", member.ProvinceCode);
            //ViewData["ProvinceCode"] = new SelectList(_context.Province, "ProvinceCode", "ProvinceCode", member.ProvinceCode);
            return View(member);
        }

        // GET: YYMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if(id.HasValue)
            {
                var _member = _context.Member.Where(m => m.MemberId == id).SingleOrDefault();
                ViewData["FullName"] = _member.FullName;
            }

            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            ViewData["ProvinceCode"] = new SelectList(_context.Province.OrderBy(p => p.Name), "ProvinceCode", "Name", member.ProvinceCode);

            return View(member);
        }

        // POST: YYMembers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MemberId,FullName,FirstName,LastName,SpouseFirstName,SpouseLastName,Street,City,ProvinceCode,PostalCode,HomePhone,Email,YearJoined,Comment,TaskExempt,UseCanadaPost")] Member member)
        {
            if (id != member.MemberId)
            {
                ModelState.AddModelError("", "The member'Id is not the one you want to update.");
                //return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                    TempData["message"] = $"Record Updated: {member.FullName}";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!MemberExists(member.MemberId))
                    {
                        ModelState.AddModelError("", $"Member is not on file {member.FullName}");
                        //return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", $"error edit the record: " + $" {ex.GetBaseException().Message}");
                        //ModelState.AddModelError("", $"error edit the record: " + $" {ex.InnerException.Message}");
                        //throw;
                    }
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("", $"update error: {ex.GetBaseException().Message}");
                }
            }
            ViewData["ProvinceCode"] = new SelectList(_context.Province, "ProvinceCode", "Name", member.ProvinceCode);
            //ViewData["ProvinceCode"] = new SelectList(_context.Province, "ProvinceCode", "ProvinceCode", member.ProvinceCode);
            return View(member);
        }

        // GET: YYMembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id.HasValue)
            {
                var _member = _context.Member.Where(m => m.MemberId == id).SingleOrDefault();
                ViewData["FullName"] = _member.FullName;
            }

            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
                .Include(m => m.ProvinceCodeNavigation)
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: YYMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Member member = _context.Member.Find(id);
                _context.Member.Remove(member);
                _context.SaveChanges();
                TempData["message"] = $"Member {member.FullName} has been deleted from database.";
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                TempData["deleteError"] = "error on deleting: " + ex.GetBaseException().Message;
            }
            return RedirectToAction("Delete", new { ID = id });
        }

        private bool MemberExists(int id)
        {
            return _context.Member.Any(e => e.MemberId == id);
        }
    }
}
