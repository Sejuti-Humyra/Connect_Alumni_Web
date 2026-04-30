using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_CRUD.Data;
using MVC_CRUD.Models;

namespace MVC_CRUD.Controllers
{
    public class AlumniController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AlumniController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================
        // SEARCH PAGE (EMPTY BY DEFAULT)
        // =============================
        public IActionResult Index(string search, string role)
        {
            var users = new List<User>();

            // Only search when user types something
            if (!string.IsNullOrWhiteSpace(search) || !string.IsNullOrWhiteSpace(role))
            {
                var query = _context.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(u =>
                        u.FullName.Contains(search) ||
                        u.Email.Contains(search));
                }

                if (!string.IsNullOrWhiteSpace(role))
                {
                    query = query.Where(u => u.Role == role);
                }

                users = query
                    .OrderBy(u => u.FullName)
                    .ToList();
            }

            return View(users);
        }

        // =============================
        // PROFILE DETAILS
        // =============================
        public IActionResult Details(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            return View(user);
        }
    }
}