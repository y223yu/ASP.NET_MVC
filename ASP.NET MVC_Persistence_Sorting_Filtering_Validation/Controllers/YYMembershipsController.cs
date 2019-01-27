using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YYSail.Models;
using Microsoft.AspNetCore.Http; 

namespace YYSail.Controllers
{
    public class YYMembershipsController : Controller
    {
        private readonly SailContext _context;

        public YYMembershipsController(SailContext context)
        {
            _context = context;
        }

        // GET: YYMemberships
        public async Task<IActionResult> Index(int? id, string fullname)
        {
            if (id.HasValue && fullname != null)
            {
                ViewData["fullname"] = fullname;
                HttpContext.Session.SetInt32("memberId", id.HasValue ? int.Parse(id.ToString()) : 0);
                HttpContext.Session.SetString("fullname", fullname);

                var membership = _context.Membership.Include(m => m.Member)
                    .Include(m => m.MembershipTypeNameNavigation)
                    .Where(m => m.MemberId == id).OrderByDescending(m => m.Year);
                return View(membership);
            }
            else if (HttpContext.Session.GetInt32("memberId").HasValue && fullname == null)
            {
                var _id = Convert.ToInt32(HttpContext.Session.GetInt32("memberId").ToString());

                var ms = _context.Member.Where(m => m.MemberId == _id).SingleOrDefault();
                ViewData["fullname"] = ms.FullName;

                var membership = _context.Membership.Include(m => m.Member)
                    .Include(m => m.MembershipTypeNameNavigation)
                    .Where(m => m.MemberId == _id).OrderByDescending(m => m.Year);
                return View(membership);
            }
            else
            {
                TempData["selectmember"] = "Please select a member.";
                return RedirectToAction("Index", "Member");
            }

            //var sailContext = _context.Membership.Include(m => m.Member).Include(m => m.MembershipTypeNameNavigation);
            //return View(await sailContext.ToListAsync());
        }

        // GET: YYMemberships/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewData["fullname"] = HttpContext.Session.GetString("fullname");

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

        // GET: YYMemberships/Create
        public IActionResult Create(int id)
        {
            ViewData["fullname"] = HttpContext.Session.GetString("fullname");

            Membership membership = new Membership();
            membership.Year = DateTime.Now.Year;

            membership.MemberId = Convert.ToInt32(HttpContext.Session.GetInt32("memberId").ToString());

            //ViewData["MemberId"] = new SelectList(_context.Member, "MemberId", "FirstName");
            ViewData["MembershipTypeName"] = new SelectList(_context.MembershipType.OrderBy(m => m.MembershipTypeName), "MembershipTypeName", "MembershipTypeName");
            return View(membership);
        }

        // POST: YYMemberships/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MembershipId,MemberId,Year,MembershipTypeName,Fee,Comments,Paid")] Membership membership)
        {
            if (ModelState.IsValid)
            {
                var annualFeeStructure = _context.AnnualFeeStructure.Where(a => a.Year == membership.Year).FirstOrDefault();
                var membershipType = _context.MembershipType.Where(m => m.MembershipTypeName == membership.MembershipTypeName).FirstOrDefault();

                if (annualFeeStructure != null && membershipType != null)
                {
                    double annualFee = Convert.ToDouble(annualFeeStructure.AnnualFee);
                    double ratioOfFull = Convert.ToDouble(membershipType.RatioToFull);

                    membership.Fee = Convert.ToSingle(annualFee * ratioOfFull);
                }

                _context.Add(membership);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            //ViewData["MemberId"] = new SelectList(_context.Member, "MemberId", "FirstName", membership.MemberId);
            ViewData["MembershipTypeName"] = new SelectList(_context.MembershipType, "MembershipTypeName", "MembershipTypeName", membership.MembershipTypeName);
            return View(membership);
        }

        // GET: YYMemberships/Edit/5
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

            if (membership.Year < Convert.ToInt32(DateTime.Now.Year))
            {
                TempData["errorEdit"] = "The prior year's record cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            //ViewData["MemberId"] = new SelectList(_context.Member, "MemberId", "FirstName", membership.MemberId);
            ViewData["MembershipTypeName"] = new SelectList(_context.MembershipType.OrderBy(m => m.MembershipTypeName), "MembershipTypeName", "MembershipTypeName", membership.MembershipTypeName);
            return View(membership);
        }

        // POST: YYMemberships/Edit/5
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
            ViewData["MemberId"] = new SelectList(_context.Member, "MemberId", "FirstName", membership.MemberId);
            ViewData["MembershipTypeName"] = new SelectList(_context.MembershipType, "MembershipTypeName", "MembershipTypeName", membership.MembershipTypeName);
            return View(membership);
        }

        // GET: YYMemberships/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            ViewData["fullname"] = HttpContext.Session.GetString("fullname");
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

        // POST: YYMemberships/Delete/5
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
