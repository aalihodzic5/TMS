using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TMS.Data;
using TMS.Models;
using TMS.Models.Enums;

namespace TMS.Controllers
{
    public class OfferController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OfferController(ApplicationDbContext context)
        {
            _context = context;
        }




        // GET: /Offer/Accept/5
        [HttpGet]
        public async Task<IActionResult> Accept(int? id)
        {

            if (id == null || id <= 0)
                return BadRequest("OfferId can not be null");

            var offer = await _context.Offer.FindAsync(id);
            if (offer == null) return NotFound();

            ViewBag.OfferStates = new SelectList(Enum.GetValues(typeof(OfferState)));
            return View(offer);
        }

        // POST: /Offer/Accept
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(Offer model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.OfferStates = new SelectList(Enum.GetValues(typeof(OfferState)));
                return View(model);
            }

            _context.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }




        // GET: Offer/Index
        public async Task<IActionResult> Index()
        {
            var offers = await _context.Offer
                .Include(o => o.Job)   // učitaj povezani Job
                .ToListAsync();

            return View(offers);
        }


        // GET: Offer/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offer = await _context.Offer
                .FirstOrDefaultAsync(m => m.Id == int.Parse(id));
            if (offer == null)
            {
                return NotFound();
            }

            return View(offer);
        }

        // GET: Offer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Offer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,JobId,UserId,report,offerDate,price,offerState")] Offer offer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(offer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(offer);
        }

        // GET: Offer/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offer = await _context.Offer.FindAsync(id);
            if (offer == null)
            {
                return NotFound();
            }
            return View(offer);
        }

        // POST: Offer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,JobId,UserId,report,offerDate,price,offerState")] Offer offer)
        {
            if (int.Parse(id) != offer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(offer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OfferExists(offer.Id))
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
            return View(offer);
        }

        // GET: Offer/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var offer = await _context.Offer
                .FirstOrDefaultAsync(m => m.Id == int.Parse(id));
            if (offer == null)
            {
                return NotFound();
            }

            return View(offer);
        }

        // POST: Offer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var offer = await _context.Offer.FindAsync(id);
            if (offer != null)
            {
                _context.Offer.Remove(offer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OfferExists(int id)
        {
            return _context.Offer.Any(e => e.Id == id);
        }


        //POST : SendOffer

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Dispatcher")]
        public async Task<IActionResult> SendOffer(int jobId, decimal offerPrice) 
        {
            var job = await _context.Job.FindAsync(jobId);

            if (job == null)
            {
               return NotFound();
            }

            string userClaim = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (userClaim == null)
            {
                return Unauthorized(); // Ako nije prijavljen, vrati Unauthorized
            }
            string userId = userClaim;

            int userOfferCount = await _context.Offer.CountAsync(o => o.JobId == jobId && o.UserId == userId);

            if (userOfferCount >= 2)
            {
                TempData["ErrorMessage"] = "Max number of offers is 2";
                return RedirectToAction("Filter", "Job");
            }

            if(offerPrice == 0)offerPrice = job.price; // Ako je cijena 0, postavi na cijenu posla

            var offer = new Offer
            {
                JobId = jobId,
                UserId = userId,
                price = offerPrice,
                offerDate = DateTime.Now,
                offerState = OfferState.PENDING,
                report = "Offer sent by " + User.Identity!.Name + " for job " + jobId,

            };

            _context.Add(offer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Offer successfully sent for job ID: " + jobId;

            return RedirectToAction("Index", "Offer");


        }



    }
}
