using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YSSail.Models;

namespace YSSail.Controllers
{
    public class YSAnnualFeeStructuresController : Controller
    {
        private readonly SailContext _context;

        public YSAnnualFeeStructuresController(SailContext context)
        {
            _context = context;
        }

        // GET: YSAnnualFeeStructures
        public async Task<IActionResult> Index()
        {
            return View(await _context.AnnualFeeStructure.OrderByDescending(f => f.Year).ToListAsync());
        }

        // GET: YSAnnualFeeStructures/Details/5
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

        // GET: YSAnnualFeeStructures/Create
        public IActionResult Create()
        {

            AnnualFeeStructure annualFeeStructures = _context.AnnualFeeStructure.OrderByDescending(a => a.Year).FirstOrDefault();

            if (annualFeeStructures != null)
            {
                annualFeeStructures.Year = DateTime.Now.Year;
            }

            return View(annualFeeStructures);
        }

        // POST: YSAnnualFeeStructures/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Year,AnnualFee,EarlyDiscountedFee,EarlyDiscountEndDate,RenewDeadlineDate,TaskExemptionFee,SecondBoatFee,ThirdBoatFee,ForthAndSubsequentBoatFee,NonSailFee,NewMember25DiscountDate,NewMember50DiscountDate,NewMember75DiscountDate")] AnnualFeeStructure annualFeeStructure)
        {
            if (ModelState.IsValid)
            {
                _context.Add(annualFeeStructure);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(annualFeeStructure);
        }

        // GET: YSAnnualFeeStructures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var annualFeeStructure = await _context.AnnualFeeStructure.FindAsync(id);
            if (annualFeeStructure == null)
            {
                TempData["EditOldRecordError"] = "Record Does not exist. Please try again with a valid record.";
                RedirectToAction("Index");
            }

            if (annualFeeStructure.Year != DateTime.Now.Year)
            {
                TempData["EditOldRecordError"] = "Please edit recoreds of th current year only.";
                return RedirectToAction("Index", "YSAnnualFeeStructures");
            }

            return View(annualFeeStructure);
        }

        // POST: YSAnnualFeeStructures/Edit/5
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

        // GET: YSAnnualFeeStructures/Delete/5
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

        // POST: YSAnnualFeeStructures/Delete/5
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
