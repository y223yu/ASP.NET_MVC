using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YYSail.Models;

namespace YYSail.Controllers
{
    public class YYAnnualFeeStructuresController : Controller
    {
        private readonly SailContext _context;

        public YYAnnualFeeStructuresController(SailContext context)
        {
            _context = context;
        }

        // GET: YYAnnualFeeStructures
        public async Task<IActionResult> Index()
        {
            var annualFeeStructureContext = _context.AnnualFeeStructure.OrderByDescending(a => a.Year);
            return View(await annualFeeStructureContext.ToListAsync());
        }

        // GET: YYAnnualFeeStructures/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var annualFeeStructure = await _context.AnnualFeeStructure
                .FirstOrDefaultAsync(m => m.Year == id);
            if (annualFeeStructure == null)
            {
                return NotFound();
            }

            return View(annualFeeStructure);
        }

        // GET: YYAnnualFeeStructures/Create
        public async Task<IActionResult> Create()
        {
            var annualFeeStructure = _context.AnnualFeeStructure.FirstOrDefault();
            if (annualFeeStructure != null)
            {
                annualFeeStructure.Year = DateTime.Now.Year;
            }

            //if (Request.Cookies["AnnualFee"] != null)
            //{
            //    AnnualFee = Convert.ToInt32(Request.Cookies["AnnualFee"]);
            //    ViewBag.AnnualFee = AnnualFee;
            //}

            return View(annualFeeStructure);
        }

        // POST: YYAnnualFeeStructures/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Year,AnnualFee,EarlyDiscountedFee,EarlyDiscountEndDate,RenewDeadlineDate,TaskExemptionFee,SecondBoatFee,ThirdBoatFee,ForthAndSubsequentBoatFee,NonSailFee,NewMember25DiscountDate,NewMember50DiscountDate,NewMember75DiscountDate")] AnnualFeeStructure annualFeeStructure)
        {
            if (ModelState.IsValid)
            {
                var cookieOptions = new CookieOptions { Expires = DateTime.Today.AddDays(70) };
                Response.Cookies.Append("Year", annualFeeStructure.Year.ToString(), cookieOptions);
                Response.Cookies.Append("AnnualFee", annualFeeStructure.AnnualFee.ToString(), cookieOptions);
                Response.Cookies.Append("EarlyDiscountedFee", annualFeeStructure.EarlyDiscountedFee.ToString(), cookieOptions);
                Response.Cookies.Append("EarlyDiscountEndDate", annualFeeStructure.EarlyDiscountEndDate.ToString(), cookieOptions);
                Response.Cookies.Append("RenewDeadlineDate", annualFeeStructure.RenewDeadlineDate.ToString(), cookieOptions);
                Response.Cookies.Append("TaskExemptionFee", annualFeeStructure.TaskExemptionFee.ToString(), cookieOptions);
                Response.Cookies.Append("SecondBoatFee", annualFeeStructure.SecondBoatFee.ToString(), cookieOptions);
                Response.Cookies.Append("ThirdBoatFee", annualFeeStructure.ThirdBoatFee.ToString(), cookieOptions);
                Response.Cookies.Append("ForthAndSubsequentBoatFee", annualFeeStructure.ForthAndSubsequentBoatFee.ToString(), cookieOptions);
                Response.Cookies.Append("NonSailFee", annualFeeStructure.NonSailFee.ToString(), cookieOptions);
                Response.Cookies.Append("NewMember25DiscountDate", annualFeeStructure.NewMember25DiscountDate.ToString(), cookieOptions);
                Response.Cookies.Append("NewMember50DiscountDate", annualFeeStructure.NewMember50DiscountDate.ToString(), cookieOptions);
                Response.Cookies.Append("NewMember75DiscountDate", annualFeeStructure.NewMember75DiscountDate.ToString(), cookieOptions);

                _context.Add(annualFeeStructure);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(annualFeeStructure);
        }

        // GET: YYAnnualFeeStructures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var annualFeeStructure = await _context.AnnualFeeStructure.FindAsync(id);
            if (annualFeeStructure == null)
            {
                TempData["error"] = "Record Does not exist. Please try again with a valid record.";
                RedirectToAction("Index");
            }

            if (annualFeeStructure.Year < Convert.ToInt32(DateTime.Now.Year))
            {
                TempData["error"] = "Error: You cannot edit prior year's record";
                return RedirectToAction(nameof(Index));
            }

            return View(annualFeeStructure);
        }

        // POST: YYAnnualFeeStructures/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Year,AnnualFee,EarlyDiscountedFee,EarlyDiscountEndDate,RenewDeadlineDate,TaskExemptionFee,SecondBoatFee,ThirdBoatFee,ForthAndSubsequentBoatFee,NonSailFee,NewMember25DiscountDate,NewMember50DiscountDate,NewMember75DiscountDate")] AnnualFeeStructure annualFeeStructure)
        {
            if (id != annualFeeStructure.Year)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(annualFeeStructure);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnnualFeeStructureExists(annualFeeStructure.Year))
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
            return View(annualFeeStructure);
        }

        // GET: YYAnnualFeeStructures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var annualFeeStructure = await _context.AnnualFeeStructure
                .FirstOrDefaultAsync(m => m.Year == id);
            if (annualFeeStructure == null)
            {
                return NotFound();
            }

            return View(annualFeeStructure);
        }

        // POST: YYAnnualFeeStructures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var annualFeeStructure = await _context.AnnualFeeStructure.FindAsync(id);
            _context.AnnualFeeStructure.Remove(annualFeeStructure);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnnualFeeStructureExists(int id)
        {
            return _context.AnnualFeeStructure.Any(e => e.Year == id);
        }
    }
}
