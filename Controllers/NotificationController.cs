using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TMS.Data;
using TMS.Models;

namespace TMS.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly UserManager<User> _userManager;


        public NotificationController(ApplicationDbContext context, IHubContext<NotificationHub> hub, UserManager<User> userManager)
        {
            _context = context;
            _hub = hub;
            _userManager = userManager;
        }


        // GET: Notification
        public async Task<IActionResult> IndexAdmin()
        {
            return View(await _context.Notification.ToListAsync());
        }

        public async Task<IActionResult> IndexUser()
        {
            var userId = _userManager.GetUserId(User);  
            var notifications = await _context.Notification
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.NotificationDate)
                .ToListAsync();
            return View(notifications);
        }

        // GET: Notification/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notification
                .FirstOrDefaultAsync(m => m.Id == int.Parse(id));
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // GET: Notification/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Notification/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Message,NotificationDate,status,Link")] Notification notification)
        {
            if (!ModelState.IsValid) return View(notification);

            _context.Add(notification);
            await _context.SaveChangesAsync();

            var senderUserId = _userManager.GetUserId(User);


            await _hub.Clients.User(notification.UserId)
                      .SendAsync("ReceiveNotification", senderUserId, notification.Message, notification.Link);

            return RedirectToAction(nameof(Index));

        }


        // GET: Notification/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notification
                .FirstOrDefaultAsync(m => m.Id == int.Parse(id));
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notification/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var notification = await _context.Notification.FindAsync(id);
            if (notification != null)
            {
                _context.Notification.Remove(notification);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificationExists(int id)
        {
            return _context.Notification.Any(e => e.Id == id);
        }
    }
}
