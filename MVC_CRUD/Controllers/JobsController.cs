using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_CRUD.Data;
using MVC_CRUD.Models;
using System;
using System.Linq;

namespace MVC_CRUD.Controllers
{
    public class JobsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public JobsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: Jobs
        public IActionResult Index()
        {
            var jobs = _context.Jobs
                .OrderByDescending(j => j.PostedAt)
                .ToList();

            return View(jobs);
        }

        // ✅ POST: Jobs/Create (for partial form AJAX)
        [HttpPost]
        public IActionResult Create(Job job)
        {
            if (ModelState.IsValid)
            {
                job.PostedAt = DateTime.Now;

                // 🔥 Set logged-in user (adjust if needed)
                job.PostedById = HttpContext.Session.GetInt32("UserId") ?? 1;

                _context.Jobs.Add(job);
                _context.SaveChanges();

                var postedBy = _context.Users
                    .Where(u => u.Id == job.PostedById)
                    .Select(u => u.FullName)
                    .FirstOrDefault();

                return Json(new
                {
                    success = true,
                    job = new
                    {
                        job.Id,
                        job.Title,
                        job.Company,
                        job.Location,
                        job.Type,
                        job.Description,
                        job.ApplyUrl,
                        postedAt = job.PostedAt.ToString("dd MMM yyyy"),
                        postedBy
                    }
                });
            }

            return Json(new { success = false });
        }
        public async Task<IActionResult> Details(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.PostedBy)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }
    }
}