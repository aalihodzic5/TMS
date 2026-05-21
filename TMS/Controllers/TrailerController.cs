using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMS.Data;
using TMS.Models;

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

        // Helper: puni dropdown kamiona samo za current usera
        private async Task PopulateTruckDropdownAsync(string userId, int? selectedTruckId = null)
        {
            var trucks = await _context.Truck
                .Where(t => t.UserID == userId)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.licensePlate,
                    Selected = (selectedTruckId.HasValue && t.Id == selectedTruckId.Value)
                })
                .ToListAsync();

            ViewBag.TruckList = trucks; // List<SelectListItem>
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
            if(userId == null) return NotFound();
            await PopulateTruckDropdownAsync(userId);
            return View(new Trailer());
        }



        // POST: Trailer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("brand,trailerType,licensePlate,registration,nextServiceDate,specification,TruckId")] Trailer trailer)
        {
            var userId = _userManager.GetUserId(User);

            if (trailer.TruckId == 0)
                ModelState.AddModelError("TruckId", "Morate izabrati kamion.");

            var isMine = await _context.Truck.AnyAsync(t => t.Id == trailer.TruckId && t.UserID == userId);
            if (!isMine)
                ModelState.AddModelError("TruckId", "Odabrani kamion ne pripada vašem nalogu.");

            var errs = ModelState.Where(kv => kv.Value.Errors.Any())
                     .Select(kv => $"{kv.Key}: {string.Join(", ", kv.Value.Errors.Select(e => e.ErrorMessage))}");
            TempData["ModelErrors"] = string.Join(" | ", errs);

            if (!ModelState.IsValid)
            {
                if(userId == null) return NotFound();
                await PopulateTruckDropdownAsync(userId, trailer.TruckId);
                return View(trailer);
            }

            _context.Add(trailer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Trailer/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            var trailer = await _context.Trailer
                .Include(t => t.Truck)
                .FirstOrDefaultAsync(t => t.Id == id && t.Truck != null && t.Truck.UserID == userId);

            if (trailer == null || userId == null) return NotFound();
            await PopulateTruckDropdownAsync(userId, trailer.TruckId);
            return View(trailer);
        }

        // POST: Trailer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,brand,trailerType,licensePlate,registration,nextServiceDate,specification,TruckId")] Trailer trailer)
        {
            if (id != trailer.Id) return NotFound();

            var userId = _userManager.GetUserId(User);

            // 1) Trailer mora biti "moj"
            var trailerIsMine = await _context.Trailer
                .Include(t => t.Truck)
                .AnyAsync(t => t.Id == id && t.Truck != null && t.Truck.UserID == userId);
            if (!trailerIsMine)
            {
                return NotFound(); // ili Forbid()
            }

            // 2) Odabrani TruckId takođe mora biti moj
            if (trailer.TruckId == 0 ||
                !await _context.Truck.AnyAsync(t => t.Id == trailer.TruckId && t.UserID == userId))
            {
                ModelState.AddModelError("TruckId", "Odabrani kamion ne pripada vašem nalogu.");
            }

            if (!ModelState.IsValid)
            {
                if(userId == null) return NotFound();
                await PopulateTruckDropdownAsync(userId, trailer.TruckId);
                return View(trailer);
            }

            try
            {
                _context.Update(trailer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TrailerExists(trailer.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Trailer/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            var trailer = await _context.Trailer
                .Include(t => t.Truck)
                .FirstOrDefaultAsync(m => m.Id == id && m.Truck != null && m.Truck.UserID == userId);

            if (trailer == null) return NotFound();

            return View(trailer);
        }

        // POST: Trailer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var trailer = await _context.Trailer
                .Include(t => t.Truck)
                .FirstOrDefaultAsync(t => t.Id == id && t.Truck != null && t.Truck.UserID == userId);

            if (trailer != null)
            {
                _context.Trailer.Remove(trailer);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TrailerExists(int id) => _context.Trailer.Any(e => e.Id == id);
    }
}
