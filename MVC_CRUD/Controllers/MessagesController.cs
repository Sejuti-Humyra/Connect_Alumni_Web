using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_CRUD.Controllers;
using MVC_CRUD.Data;
using MVC_CRUD.Models;

[Route("Messages")]
public class MessagesController : BaseController
{
    private readonly ApplicationDbContext _context;

    public MessagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ================= INBOX =================
    [HttpGet("")]
    public IActionResult Index()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Account");

        var users = _context.Users
            .Where(u => u.Id != userId)
            .ToList();

        var conversations = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .ToList()
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => g.OrderByDescending(x => x.SentAt).First())
            .OrderByDescending(x => x.SentAt)
            .ToList();

        ViewBag.Users = users;
        ViewBag.Conversations = conversations;
        ViewBag.CurrentUserId = userId;

        return View();
    }

    // ================= OPEN CHAT =================
    [HttpGet("Conversation/{id}")]
    public IActionResult Conversation(int id)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Account");

        var messages = _context.Messages
            .Where(m =>
                (m.SenderId == userId && m.ReceiverId == id) ||
                (m.SenderId == id && m.ReceiverId == userId))
            .OrderBy(m => m.SentAt)
            .ToList();

        var otherUser = _context.Users.FirstOrDefault(u => u.Id == id);

        var conversations = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .ToList()
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => g.OrderByDescending(x => x.SentAt).First())
            .OrderByDescending(x => x.SentAt)
            .ToList();

        ViewBag.Messages = messages;
        ViewBag.OtherUser = otherUser;
        ViewBag.CurrentUserId = userId;
        ViewBag.ActiveUserId = id;
        ViewBag.Conversations = conversations;

        return View("Index");
    }

    // ================= SEND MESSAGE =================
    [HttpPost("Send")]
    public IActionResult Send(int receiverId, string content)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var msg = new Message
        {
            SenderId = userId.Value,
            ReceiverId = receiverId,
            Content = content,
            SentAt = DateTime.Now,
            IsRead = false
        };

        _context.Messages.Add(msg);
        _context.SaveChanges();

        return Json(new { success = true });
    }

    // ================= GET CHAT MESSAGES =================
    [HttpGet("GetMessages")]
    public IActionResult GetMessages(int userId)
    {
        int? myId = HttpContext.Session.GetInt32("UserId");
        if (myId == null) return Unauthorized();

        var messages = _context.Messages
            .Where(m =>
                (m.SenderId == myId && m.ReceiverId == userId) ||
                (m.SenderId == userId && m.ReceiverId == myId))
            .OrderBy(m => m.SentAt)
            .Select(m => new
            {
                senderId = m.SenderId,
                content = m.Content
            })
            .ToList();

        return Json(messages);
    }
}