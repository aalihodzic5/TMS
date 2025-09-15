using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMS.Data;
using TMS.Models;
using Microsoft.AspNetCore.Identity;



namespace TMS.Controllers
{
    [Authorize(Roles = "Dispatcher, Administrator")]
    public class TrailerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public TrailerController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task PopulateTruckDropdownAsync(string userId, int? selectedTruckId = null)
        {
            var trucks = await _context.Truck
                .Where(t => t.UserID == userId)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.licensePlate,
                    Selected = (selectedTruckId != null && t.Id == selectedTruckId)
                })
                .ToListAsync();

            ViewBag.TruckList = trucks;  //  List<SelectListItem>
        }


        // GET: Trailer
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var trailers = await _context.Trailer
            .Include(t => t.Truck)
            .Where(t => t.Truck != null && t.Truck.UserID == userId)
            .ToListAsync();

            return View(trailers);
        }

        // GET: Trailer/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var trailer = await _context.Trailer
                .Include(t => t.Truck)
                .FirstOrDefaultAsync(m => m.Id == id && m.Truck != null && m.Truck.UserID == userId);

            if (trailer == null) return NotFound();
            return View(trailer);
        }

        // GET: Trailer/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            await PopulateTruckDropdownAsync(userId);
            return View();
        }

        // POST: Trailer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,brand,trailerType,licensePlate,registration,nextServiceDate,specification,TruckId")] Trailer trailer)
        {
            var userId = _userManager.GetUserId(User);

            // serverska validacija: TruckId mora pripadati useru
            if (trailer.TruckId != 0) // Fix: Replaced HasValue with a check for non-zero value
            {
                var isMine = await _context.Truck.AnyAsync(t => t.Id == trailer.TruckId && t.UserID == userId);
                if (!isMine)
                    ModelState.AddModelError("TruckId", "Odabrani kamion ne pripada vašem nalogu.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateTruckDropdownAsync(userId, trailer.TruckId);
                return View(trailer);
            }

            _context.Add(trailer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
