using Microsoft.AspNetCore.Mvc;
using MVC_CRUD.Data;

namespace MVC_CRUD.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalStudents = _context.Users.Count(u => u.Role == "Student");
            ViewBag.TotalAlumni = _context.Users.Count(u => u.Role == "Alumni");

            return View();
        }
    }
}
