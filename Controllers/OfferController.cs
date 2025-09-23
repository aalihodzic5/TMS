using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TMS.Data;
using TMS.Models;
using TMS.Models.Enums;
using Microsoft.AspNetCore.SignalR;

namespace TMS.Controllers
{
    public class OfferController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public OfferController(ApplicationDbContext context, IHubContext<NotificationHub> notificationHub)
        {
            _context = context;
            _notificationHub = notificationHub;
        }

        // GET: Offer
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var offers = await _context.Offer.Include(o => o.User).Include(o => o.Job).ToListAsync();
            return View(offers);
        }


        // POST : SendOffer
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Dispatcher")]
        public async Task<IActionResult> SendOffer(int jobId, decimal offerPrice)
        {
            var job = await _context.Job.Include(j => j.User).FirstOrDefaultAsync(j => j.Id == jobId);
            if (job == null) return NotFound();

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            int userOfferCount = await _context.Offer.CountAsync(o => o.JobId == jobId && o.UserId == userId);
            if (userOfferCount >= 2)
            {
                TempData["ErrorMessage"] = "Max number of offers is 2";
                return RedirectToAction("Filter", "Job");
            }

            if (offerPrice == 0) offerPrice = job.price;

            var offer = new Offer
            {
                JobId = jobId,
                UserId = userId,
                price = offerPrice,
                offerDate = DateTime.Now,
                offerState = OfferState.PENDING,
                report = "Offer sent by " + User.Identity!.Name + " for job " + jobId
            };

            Console.WriteLine($"JobId: {jobId}, UserId: {userId}, OfferPrice: {offerPrice}, brokerId: {job.UserId}");

            _context.Add(offer);
            await _context.SaveChangesAsync();

            // --- NOTIFIKACIJA ---
            var brokerId = job.UserId; // pretpostavljamo da broker drži posao
            var notification = new Notification
            {
                UserId = brokerId,
                Message = $"New offer sent by {User.Identity!.Name} for job {jobId}.",
                NotificationDate = DateTime.Now,
                status = NotificationStatus.UNREAD,
                Link = "/Job/Details/" + jobId
            };
            _context.Notification.Add(notification);
            await _context.SaveChangesAsync();

            // Pošalji putem SignalR
            await _notificationHub.Clients.User(brokerId)
                .SendAsync("ReceiveNotification", notification.Message, "/Job/Details/" + jobId);

            TempData["SuccessMessage"] = "Offer successfully sent for job ID: " + jobId;
            return RedirectToAction("Index", "Offer");
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Broker")]
        public async Task<IActionResult> Accept(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            var offer = await _context.Offer.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);
            if (offer == null) return NotFound();

            offer.offerState = OfferState.ACCEPTED;
            _context.Update(offer);
            await _context.SaveChangesAsync();

            // NOTIFIKACIJA za korisnika koji je poslao ponudu
            var notification = new Notification
            {
                UserId = offer.UserId,
                Message = $"Your offer for job {offer.JobId} has been ACCEPTED by {User.Identity!.Name}.",
                NotificationDate = DateTime.Now,
                status = NotificationStatus.UNREAD,
                Link = "/Offer/Details/" + offer.Id
            };
            _context.Notification.Add(notification);
            await _context.SaveChangesAsync();

            await _notificationHub.Clients.User(offer.UserId)
                .SendAsync("ReceiveNotification", notification.Message, "/Offer/Details/" + offer.Id);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Broker")]
        public async Task<IActionResult> Reject(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            var offer = await _context.Offer.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);
            if (offer == null) return NotFound();

            offer.offerState = OfferState.REJECTED;
            _context.Update(offer);
            await _context.SaveChangesAsync();

            // NOTIFIKACIJA za korisnika koji je poslao ponudu
            var notification = new Notification
            {
                UserId = offer.UserId,
                Message = $"Your offer for job {offer.JobId} has been REJECTED by {User.Identity!.Name}.",
                NotificationDate = DateTime.Now,
                status = NotificationStatus.UNREAD,
                Link = "/Offer/Details/" + offer.Id
            };
            _context.Notification.Add(notification);
            await _context.SaveChangesAsync();

            await _notificationHub.Clients.User(offer.UserId)
                .SendAsync("ReceiveNotification", notification.Message, "/Offer/Details/" + offer.Id);

            return RedirectToAction("Index");
        }
    }
}
