using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using MVC_CRUD.Data;
using MVC_CRUD.Models;

namespace Pages.Posts
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public CreateModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public Post Post { get; set; } = new Post();

        public void OnGet()
        {
            // Render empty form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            Post.CreatedAt = DateTime.Now;
            Post.UserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            _db.Posts.Add(Post);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Post created successfully.";
            return RedirectToPage("/Index");
        }
    }
}