using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MVC_CRUD.Models;
using MVC_CRUD.Data;

namespace MVC_CRUD.Controllers
{
    public class PostsController : BaseController
    {
        private readonly ApplicationDbContext _db;

        public PostsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(IEnumerable<Post>? posts = null)
        {
            var model = posts?.ToList()
                        ?? _db.Posts
                              .Include(p => p.User)
                              .Include(p => p.Likes)
                              .Include(p => p.Comments).ThenInclude(c => c.User)
                              .OrderByDescending(p => p.CreatedAt)
                              .ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? content, string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Post content cannot be empty.";
                return RedirectToAction(nameof(Index));
            }

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var post = new Post
            {
                Content = content,
                ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl,
                CreatedAt = DateTime.Now,
                UserId = userId
            };

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            TempData["NewPostId"] = post.Id;
            TempData["Success"] = "Post created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction(nameof(Index));
            }

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var comment = new Comment
            {
                Content = content,
                CreatedAt = DateTime.Now,
                PostId = postId,
                UserId = userId
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            TempData["NewPostId"] = postId;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var userRole = HttpContext.Session.GetString("UserRole");

            var comment = await _db.Comments.FindAsync(id);
            if (comment == null)
            {
                TempData["Error"] = "Comment not found.";
                return RedirectToAction(nameof(Index));
            }

            if (comment.UserId != userId && userRole != "Admin")
            {
                TempData["Error"] = "You are not allowed to delete this comment.";
                return RedirectToAction(nameof(Index));
            }

            int postId = comment.PostId;
            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();

            TempData["NewPostId"] = postId;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
            {
                TempData["Error"] = "You must be signed in to like posts.";
                return RedirectToAction(nameof(Index));
            }

            // Find existing like
            var existingLike = await _db.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null)
            {
                _db.Likes.Remove(existingLike);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Like removed.";
            }
            else
            {
                var like = new Like
                {
                    PostId = postId,
                    UserId = userId
                    // set other fields if your Like model requires them
                };

                _db.Likes.Add(like);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Post liked.";
            }

            // Keep UI focused on the post
            TempData["NewPostId"] = postId;
            return RedirectToAction(nameof(Index));
        }
    }
}