using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TMS.Data;
using TMS.Models;

namespace TMS.Controllers
{
    [Authorize]
    [Authorize(Roles = "Dispatcher, Administrator")]
    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public DriverController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Driver
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var drivers = await _context.Driver
                .Include(d => d.Truck)
                .Where(d =>
                    d.UserID == userId ||
                    (d.TruckId != null && _context.Truck.Any(t => t.Id == d.TruckId && t.UserID == userId))
                )
                .ToListAsync();

            return View(drivers);
        }



        // GET: Driver/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var driver = await _context.Driver
                .Include(d => d.Truck)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (driver == null)
                return NotFound();

            return View(driver);
        }

        
        // GET: Driver/Create
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            if (user == null || userId == null) 
                { return Unauthorized(); }

            ViewBag.CurrentUserName = $"{user.Ime} {user.Prezime}";

            var driver = new Driver
            {
                UserID = userId
            };

            await PopulateDropdownsAsync(userId);
            return View(driver);
        }


        // POST: Driver/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,PhoneNumber,DateOfBirth,TruckId,UserID,DriverStatus")] Driver driver)
        {

            driver.UserID = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            if(user == null || driver.UserID == null) 
                { return Unauthorized(); }
            ViewBag.CurrentUserName = $"{user.Ime} {user.Prezime}"; 
            ViewBag.CurrentUserID = user.Id;

            if (ModelState.IsValid)
            {
                _context.Add(driver);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(driver.UserID, driver.TruckId);
            return View(driver);
        }

        // GET: Driver/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var driver = await _context.Driver.FindAsync(id);
            if (driver == null || driver.UserID==null)
                return NotFound();

            await PopulateDropdownsAsync(driver.UserID, driver.TruckId);
            return View(driver);
        }

        // POST: Driver/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,PhoneNumber,DateOfBirth,TruckId,UserID,DriverStatus")] Driver driver)
        {
            if (id != driver.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(driver);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriverExists(driver.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(driver.UserID, driver.TruckId);
            return View(driver);
        }

        // GET: Driver/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var driver = await _context.Driver
                .Include(d => d.Truck)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (driver == null)
                return NotFound();

            return View(driver);
        }

        // POST: Driver/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driver = await _context.Driver.FindAsync(id);
            if (driver != null)
            {
                _context.Driver.Remove(driver);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DriverExists(int id)
        {
            return _context.Driver.Any(e => e.Id == id);
        }



        private async Task PopulateDropdownsAsync(string? selectedUserId = null, int? selectedTruckId = null)
        {
            var users = await _userManager.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = (u.Ime ?? "") + " " + (u.Prezime ?? "")
                })
                .ToListAsync();

            var trucks = await _context.Truck
                .Where(t => t.UserID == selectedUserId)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.licensePlate,
                    Selected = (selectedTruckId != null && t.Id == selectedTruckId) 
                })
                .ToListAsync();

            ViewBag.Users = users;
            ViewBag.Trucks = trucks;
        }


    }
}

