using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMS.Data;
using TMS.Models;

namespace TMS.Controllers
{
    public class TrailerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrailerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trailer
        public async Task<IActionResult> Index()
        {
            return View(await _context.Trailer.ToListAsync());
        }

        // GET: Trailer/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trailer = await _context.Trailer
                .FirstOrDefaultAsync(m => m.Id == int.Parse(id));
            if (trailer == null)
            {
                return NotFound();
            }

            return View(trailer);
        }

        // GET: Trailer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Trailer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,brand,trailerType,licensePlate,registration,nextServiceDate,specification,TruckId")] Trailer trailer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trailer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(trailer);
        }

        // GET: Trailer/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trailer = await _context.Trailer.FindAsync(id);
            if (trailer == null)
            {
                return NotFound();
            }
            return View(trailer);
        }

        // POST: Trailer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,brand,trailerType,licensePlate,registration,nextServiceDate,specification,TruckId")] Trailer trailer)
        {
            if (int.Parse(id) != trailer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trailer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrailerExists(trailer.Id))
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
            return View(trailer);
        }

        // GET: Trailer/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trailer = await _context.Trailer
                .FirstOrDefaultAsync(m => m.Id == int.Parse(id));
            if (trailer == null)
            {
                return NotFound();
            }

            return View(trailer);
        }

        // POST: Trailer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var trailer = await _context.Trailer.FindAsync(id);
            if (trailer != null)
            {
                _context.Trailer.Remove(trailer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrailerExists(int id)
        {
            return _context.Trailer.Any(e => e.Id == id);
        }
    }
}
