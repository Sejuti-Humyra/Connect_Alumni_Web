using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_CRUD.Data;
using MVC_CRUD.Models;
using MVC_CRUD.ViewModels;

namespace MVC_CRUD.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // ADMIN DASHBOARD
        // =====================================================
        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Check Admin Role
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var vm = new AdminDashboardViewModel();

            // =====================================================
            // TOTAL COUNTS
            // =====================================================
            vm.TotalJobs = await _context.Jobs.CountAsync();
            vm.TotalUsers = await _context.Users.CountAsync();

            // =====================================================
            // JOBS TODAY
            // =====================================================
            var today = DateTime.UtcNow.Date;

            vm.JobsToday = await _context.Jobs
                .CountAsync(j => j.PostedAt >= today);

            // =====================================================
            // RECENT JOBS
            // =====================================================
            vm.RecentJobs = await _context.Jobs
                .Include(j => j.PostedBy)
                .OrderByDescending(j => j.PostedAt)
                .Take(8)
                .ToListAsync();

            // =====================================================
            // RECENT USERS
            // =====================================================
            vm.RecentUsers = await _context.Users
                .OrderByDescending(u => u.Id)
                .Take(6)
                .ToListAsync();

            // =====================================================
            // LAST 7 DAYS JOB CHART
            // =====================================================
            var fromDate = today.AddDays(-6);
            var toDate = today.AddDays(1);

            var jobsRange = await _context.Jobs
                .Where(j => j.PostedAt >= fromDate &&
                            j.PostedAt < toDate)
                .ToListAsync();

            var byDate = jobsRange
                .GroupBy(j => j.PostedAt.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            for (int i = 6; i >= 0; i--)
            {
                var d = today.AddDays(-i);

                vm.Last7Labels.Add(d.ToString("dd MMM"));

                vm.Last7Data.Add(
                    byDate.ContainsKey(d)
                        ? byDate[d]
                        : 0
                );
            }

            // =====================================================
            // JOB TYPE CHART
            // =====================================================
            var byType = await _context.Jobs
                .GroupBy(j =>
                    string.IsNullOrWhiteSpace(j.Type)
                        ? "Other"
                        : j.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            vm.JobTypeLabels = byType
                .Select(x => x.Type)
                .ToList();

            vm.JobTypeData = byType
                .Select(x => x.Count)
                .ToList();

            return View(vm);
        }

        // =====================================================
        // DELETE JOB PAGE
        // =====================================================
        // GET: /Admin/DeleteJob/5
        public async Task<IActionResult> DeleteJob(int id)
        {
            // Check Admin Role
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var job = await _context.Jobs
                .Include(j => j.PostedBy)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // =====================================================
        // CONFIRM DELETE JOB
        // =====================================================
        // POST: /Admin/ConfirmDeleteJob
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeleteJob(int id)
        {
            // Check Admin Role
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            _context.Jobs.Remove(job);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Dashboard));
        }
    }
}