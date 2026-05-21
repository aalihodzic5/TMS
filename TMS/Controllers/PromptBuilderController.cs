using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS.Data;
using GeminiPromptGenerator; // za PromptBuilder
using TMS.Services; // za GeminiService

namespace TMS.Controllers
{
    public class PromptBuilderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GeminiService _geminiService;

        public PromptBuilderController(ApplicationDbContext context, GeminiService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        [HttpGet]
        public async Task<IActionResult> GeminiAnalyze(int maxResults = 5)
        {
            var jobs = await _context.Job
                .AsNoTracking()
                .Take(50) // limit, možeš i maknuti
                .ToListAsync();

            // generiši prompt direktno iz Job modela
            string prompt = PromptBuilder.BuildGeminiPrompt(jobs, "EUR", maxResults);

            // šalji Gemini-ju
            string geminiResponse = await _geminiService.SendPromptAsync(prompt);

            ViewBag.GeminiResult = geminiResponse;

            return View("GeminiAnalyze");
        }
    }
}
