using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMS.Data;
using TMS.Models;

namespace TMS.Controllers
{
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
            return View(await _context.Driver.Include(d => d.Truck).ToListAsync());
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
            

            await SetDropdownListsAsync();
            return View();
        }

        // POST: Driver/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,PhoneNumber,DateOfBirth,TruckId,UserID,DriverStatus")] Driver driver)
        {
            if (ModelState.IsValid)
            {
                _context.Add(driver);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

           

            await SetDropdownListsAsync(driver);
            return View(driver);
        }

        // GET: Driver/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var driver = await _context.Driver.FindAsync(id);
            if (driver == null)
                return NotFound();

            ViewBag.Trucks = new SelectList(_context.Truck.ToList(), "Id", "RegistrationNumber", driver.TruckId);
            return View(driver);
        }

        // POST: Driver/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,PhoneNumber,DateOfBirth,TruckId,DriverStatus")] Driver driver)
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

            ViewBag.Trucks = new SelectList(_context.Truck.ToList(), "Id", "RegistrationNumber", driver.TruckId);
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


        private async Task SetDropdownListsAsync(Driver driver = null)
        {
            var users = await _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    FullName = u.Ime + " " + u.Prezime
                })
                .ToListAsync();

            var trucks = await _context.Truck.ToListAsync();

            ViewBag.Users = new SelectList(users, "Id", "FullName", driver?.UserID);
            ViewBag.Trucks = new SelectList(trucks, "Id", "RegistrationNumber", driver?.TruckId);
        }


        // Helper method to set dropdown lists for Create and Edit views
    }
}
