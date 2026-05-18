using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using MVC_CRUD.Data;
using MVC_CRUD.Models;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// Add services to the container
// ----------------------------

// Add MVC controllers with views
builder.Services.AddControllersWithViews();

// Add DbContext (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add session for storing user info
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register password hasher for your User entity (used in Login)
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Configure Authentication: default to cookie auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "ConnectAlumni.Auth";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

// Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// ----------------------------
// Configure the HTTP request pipeline
// ----------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Serve static files (CSS, JS, images)
app.UseStaticFiles();

// Enable routing
app.UseRouting();

// Enable session
app.UseSession();

// Authentication/Authorization middleware order is important
app.UseAuthentication();   // <-- Must come before UseAuthorization
app.UseAuthorization();

// Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Run the app
app.Run();