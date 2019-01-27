using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YSSail.Models;

namespace YSSail.Controllers
{
    public class YSMembershipsController : Controller
    {
        private readonly SailContext _context;

        public YSMembershipsController(SailContext context)
        {
            _context = context;
        }

        // GET: Memberships
        public async Task<IActionResult> Index(int? id, string fullname)
        {
            if (!id.HasValue)
            {
                if (!HttpContext.Session.GetInt32("MemberId").HasValue)
                {
                    TempData["MembershipErrors"] = "Cannot load membership data without a valid member being selected.";
                    return RedirectToAction("Index", "YSMembers");
                }

                id = HttpContext.Session.GetInt32("MemberId");
            }

            if (fullname == null)
            {
                var member = _context.Member.Where(m => m.MemberId == id).SingleOrDefault();
                ViewData["memberName"] = member.FullName;
            }

            ViewData["memberName"] = fullname;

            HttpContext.Session.SetInt32("MemberId", (int)id);
            var sailContext = _context.Membership.Include(m => m.Member)
                                        .Include(m => m.MembershipTypeNameNavigation)
                                        .Where(m => m.MemberId == id)
                                        .OrderByDescending(m => m.Year);

            return View(await sailContext.ToListAsync());
        }

        // GET: Memberships/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membership = await _context.Membership
                .Include(m => m.Member)
                .Include(m => m.MembershipTypeNameNavigation)
                .FirstOrDefaultAsync(m => m.MembershipId == id);
            if (membership == null)
            {
                return NotFound();
            }

            return View(membership);
        }

        // GET: Memberships/Create
        public IActionResult Create()
        {
            Membership _membership = new Membership();
            _membership.Year = DateTime.Now.Year;
            _membership.MemberId = (int)HttpContext.Session.GetInt32("MemberId");

            ViewData["MembershipTypeName"] = new SelectList(_context.MembershipType.OrderBy(m => m.MembershipTypeName), "MembershipTypeName", "MembershipTypeName");
            return View(_membership);
        }

        // POST: Memberships/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MembershipId,MemberId,Year,MembershipTypeName,Fee,Comments,Paid")] Membership membership)
        {
            if (ModelState.IsValid)
            {
                AnnualFeeStructure fee = _context.AnnualFeeStructure.Where(a => a.Year == membership.Year).FirstOrDefault();
                MembershipType type = _context.MembershipType.Where(m => m.MembershipTypeName == membership.MembershipTypeName).FirstOrDefault();

                if (fee != null && type != null)
                   membership.Fee = Convert.ToSingle((fee.AnnualFee.HasValue ? (double)fee.AnnualFee : 0) * type.RatioToFull);
                
                _context.Add(membership);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["MembershipTypeName"] = new SelectList(_context.MembershipType, "MembershipTypeName", "MembershipTypeName", membership.MembershipTypeName);
            return View(membership);
        }

        // GET: Memberships/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membership = await _context.Membership.FindAsync(id);
            if (membership == null)
            {
                return NotFound();
            }

            if (membership.Year != DateTime.Now.Year)
            {
                TempData["MembershipErrors"] = "Cannot edit membership records of prior years.";
                return RedirectToAction("Index", "YSMembers");
            }

            ViewData["MembershipTypeName"] = new SelectList(_context.MembershipType.OrderBy(m => m.MembershipTypeName), "MembershipTypeName", "MembershipTypeName", membership.MembershipTypeName);
            return View(membership);
        }

        // POST: Memberships/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MembershipId,MemberId,Year,MembershipTypeName,Fee,Comments,Paid")] Membership membership)
        {
            if (id != membership.MembershipId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(membership);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MembershipExists(membership.MembershipId))
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
            ViewData["MembershipTypeName"] = new SelectList(_context.MembershipType, "MembershipTypeName", "MembershipTypeName", membership.MembershipTypeName);
            return View(membership);
        }

        // GET: Memberships/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membership = await _context.Membership
                .Include(m => m.Member)
                .Include(m => m.MembershipTypeNameNavigation)
                .FirstOrDefaultAsync(m => m.MembershipId == id);
            if (membership == null)
            {
                return NotFound();
            }

            return View(membership);
        }

        // POST: Memberships/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var membership = await _context.Membership.FindAsync(id);
            _context.Membership.Remove(membership);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MembershipExists(int id)
        {
            return _context.Membership.Any(e => e.MembershipId == id);
        }
    }
}
