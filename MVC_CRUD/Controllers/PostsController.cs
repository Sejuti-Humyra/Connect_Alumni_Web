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

        // =====================================================
        // INDEX (NEWS FEED)
        // =====================================================
        public IActionResult Index(IEnumerable<Post>? posts = null)
        {
            var model = posts?.ToList()
                ?? _db.Posts
                    .Include(p => p.User)
                    .Include(p => p.Likes)
                    .Include(p => p.Comments)
                        .ThenInclude(c => c.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

            return View(model);
        }

        // =====================================================
        // CREATE POST
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? content, string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Post content cannot be empty.";
                return RedirectToAction(nameof(Index));
            }

            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || userId == 0)
            {
                TempData["Error"] = "You must be logged in.";
                return RedirectToAction(nameof(Index));
            }

            var post = new Post
            {
                Content = content.Trim(),
                ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl,
                CreatedAt = DateTime.Now,
                UserId = userId.Value
            };

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            TempData["NewPostId"] = post.Id;
            TempData["Success"] = "Post created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // ADD COMMENT
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction(nameof(Index));
            }

            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || userId == 0)
            {
                TempData["Error"] = "You must be logged in.";
                return RedirectToAction(nameof(Index));
            }

            var comment = new Comment
            {
                Content = content.Trim(),
                CreatedAt = DateTime.Now,
                PostId = postId,
                UserId = userId.Value
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            TempData["NewPostId"] = postId;

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // TOGGLE LIKE
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || userId == 0)
            {
                TempData["Error"] = "You must be signed in.";
                return RedirectToAction(nameof(Index));
            }

            var existingLike = await _db.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId.Value);

            if (existingLike != null)
            {
                _db.Likes.Remove(existingLike);
                TempData["Success"] = "Like removed.";
            }
            else
            {
                var like = new Like
                {
                    PostId = postId,
                    UserId = userId.Value
                };

                _db.Likes.Add(like);
                TempData["Success"] = "Post liked.";
            }

            await _db.SaveChangesAsync();

            TempData["NewPostId"] = postId;

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // DELETE COMMENT (SECURE)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var comment = await _db.Comments.FindAsync(id);

            if (comment == null)
            {
                TempData["Error"] = "Comment not found.";
                return RedirectToAction(nameof(Index));
            }

            bool isOwner = comment.UserId == userId;
            bool isAdmin = userRole == "Admin";

            if (!isOwner && !isAdmin)
            {
                TempData["Error"] = "Not allowed to delete this comment.";
                return RedirectToAction(nameof(Index));
            }

            int postId = comment.PostId;

            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();

            TempData["NewPostId"] = postId;

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // DELETE POST (NEW - SECURE)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userId == 0)
            {
                TempData["Error"] = "You must be logged in.";
                return RedirectToAction(nameof(Index));
            }

            var post = await _db.Posts
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                TempData["Error"] = "Post not found.";
                return RedirectToAction(nameof(Index));
            }

            bool isOwner = post.UserId == userId;
            bool isAdmin = userRole == "Admin";

            if (!isOwner && !isAdmin)
            {
                TempData["Error"] = "You are not allowed to delete this post.";
                return RedirectToAction(nameof(Index));
            }

            // remove related data first (safe cleanup)
            var comments = _db.Comments.Where(c => c.PostId == id);
            var likes = _db.Likes.Where(l => l.PostId == id);

            _db.Comments.RemoveRange(comments);
            _db.Likes.RemoveRange(likes);

            _db.Posts.Remove(post);

            await _db.SaveChangesAsync();

            TempData["Success"] = "Post deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}