using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS.Data;
using TMS.Models;

namespace TMS.Controllers
{
    [Authorize]
    public class SavedJobController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public SavedJobController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /SavedJob
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var uid = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(uid)) return Challenge();

            var items = await _context.SavedJob
                .Where(s => s.UserId == uid)
                .Include(s => s.Job)
                .OrderByDescending(s => s.savedDate)
                .ToListAsync();

            return View(items);
        }

        // POST: /SavedJob/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int jobId)
        {
            var uid = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(uid)) return Challenge();

            var exists = await _context.SavedJob
                .AnyAsync(s => s.UserId == uid && s.JobId == jobId);

            if (!exists)
            {
                _context.SavedJob.Add(new SavedJob
                {
                    UserId = uid,
                    JobId = jobId,
                    savedDate = DateTime.UtcNow
                });

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    
                }
            }

            var back = Request.Headers["Referer"].ToString();
            return !string.IsNullOrWhiteSpace(back) ? Redirect(back) : RedirectToAction(nameof(Index));
        }

        // POST: /SavedJob/Unsave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unsave(int jobId)
        {
            var uid = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(uid)) return Challenge();

            var row = await _context.SavedJob
                .FirstOrDefaultAsync(s => s.UserId == uid && s.JobId == jobId);

            if (row != null)
            {
                _context.SavedJob.Remove(row);
                await _context.SaveChangesAsync();
            }

            var back = Request.Headers["Referer"].ToString();
            return !string.IsNullOrWhiteSpace(back) ? Redirect(back) : RedirectToAction(nameof(Index));
        }

        // GET: /SavedJob/Details/5 (opcionalno)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var uid = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(uid)) return Challenge();

            var saved = await _context.SavedJob
                .Include(s => s.Job)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == uid);

            if (saved == null) return NotFound();
            return View(saved);
        }
    }
}
