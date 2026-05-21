using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS.Data;
using TMS.Models;

[Authorize]
[Route("Comparator")]
public class ComparatorController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;

    public ComparatorController(ApplicationDbContext db, UserManager<User> userManager)
    {
        _db = db; _userManager = userManager;
    }

  
    [HttpGet("")]
    public async Task<IActionResult> Index(string ids)
    {
        if (string.IsNullOrWhiteSpace(ids))
            return RedirectToAction("Filter", "Job");

        var parsed = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s, out var i) ? i : (int?)null)
                        .Where(i => i.HasValue).Select(i => i!.Value)
                        .Distinct().Take(2).ToList();

        if (parsed.Count < 2)
            return RedirectToAction("Filter", "Job");

        var jobs = await _db.Job.Where(j => parsed.Contains(j.Id)).ToListAsync();
        if (jobs.Count < 2)
            return RedirectToAction("Filter", "Job");

        var ordered = parsed.Select(id => jobs.First(j => j.Id == id)).ToList();
        return View(ordered); 
    }
}
