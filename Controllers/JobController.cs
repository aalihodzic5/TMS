using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TMS.Data;
using TMS.Models;
using TMS.Models.Enums;

using System.Text.Json;
using System.Net.Http;



namespace TMS.Controllers
{
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public JobController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }



        // GET: Job
        public async Task<IActionResult> Index()
        {
            return View(await _context.Job.ToListAsync());
        }

        // GET: Job/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var job = await _context.Job
                .FirstOrDefaultAsync(m => m.Id == int.Parse(id));
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // GET: Job/Create
        public IActionResult Create()
        {

            ViewBag.TrailerTypes = new SelectList(Enum.GetValues(typeof(TrailerTypes)));
            ViewBag.LoadTypes = new SelectList(Enum.GetValues(typeof(LoadType)));

            return View();
        }

        // POST: Job/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,loadDate,TrailerTypes,LoadType,distanceOrigin,distanceDestination,locationOrigin,locationDestination,companyName,loadWeight,loadLength,price,comments,postingDate")] Job job)
        {
            if (ModelState.IsValid)
            {
                _context.Add(job);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(job);
        }

        // GET: Job/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var job = await _context.Job.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }
            return View(job);
        }

        // POST: Job/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,loadDate,trailerType,loadType,distanceOrigin,distanceDestination,locationOrigin,locationDestination,companyName,loadWeight,loadLength,price,comments,postingDate")] Job job)
        {
            if (id != job.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(job);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobExists(job.Id))
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
            return View(job);
        }

        // GET: Job/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var job = await _context.Job
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // POST: Job/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var job = await _context.Job.FindAsync(id);
            if (job != null)
            {
                _context.Job.Remove(job);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobExists(int id)
        {
            return _context.Job.Any(e => e.Id == id);
        }


        //Filter forms


        private string GetCurrentDispatcherId()
        {
          return User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        }


        [Authorize(Roles = "Dispatcher, Administrator")]
        
        public IActionResult Filter()
        {
            string dispatcherId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            var drivers = _context.Driver
                .Where(d => d.Id.ToString() == dispatcherId)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.FirstName + " " + d.LastName
                }).ToList();

            ViewBag.DriverList = drivers;

            var jobs = _context.Job.Where(j => j.DriverId == null).ToList();

            return View(jobs);
        }

        //MAPS
        public IActionResult Map()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetRoute([FromBody] List<List<double>> coordinates)
        {
            try
            {
                var client = new HttpClient();
                var apiKey = _config["OpenRouteService:ApiKey"];
                //client.DefaultRequestHeaders.Add("Authorization", apiKey); // bez "Bearer"

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");


                var body = new { coordinates = coordinates };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.openrouteservice.org/v2/directions/driving-car", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Greška iz OpenRouteService API-ja: {errorDetails}");
                }

                var result = await response.Content.ReadAsStringAsync();
                return Content(result, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška na serveru: {ex.Message}\n{ex.StackTrace}");
            }
        }


    }
}
