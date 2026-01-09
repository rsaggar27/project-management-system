using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Api.Data;
using PMS.Api.DTOs;
using PMS.Api.Models;
using PMS.Api.Services;
using System.Security.Claims;

namespace PMS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;
    private readonly EmailDispatcher _emailDispatcher;

    public AuthController(
        AppDbContext db,
        JwtService jwt,
        EmailDispatcher emailDispatcher)
    {
        _db = db;
        _jwt = jwt;
        _emailDispatcher = emailDispatcher;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return BadRequest("Email already exists");

        var user = new User
        {
            FullName = req.FullName,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null)
            return Unauthorized("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = _jwt.GenerateToken(user);

        // 📧 Login email (best-effort, never block login)
        try
        {
            _emailDispatcher.Send(
                to: user.Email,
                subject: "New login to your account",
                body:
            $@"Hi {user.FullName},

            A new login to your account was detected.

            Time: {DateTime.UtcNow:u}
            IP: {HttpContext.Connection.RemoteIpAddress}
            Device: {Request.Headers["User-Agent"]}

            If this wasn't you, please reset your password immediately."
            );
        }
        catch (Exception ex)
        {
            // Intentionally ignored
            Console.WriteLine($"[LOGIN EMAIL FAILED] {ex.Message}");
        }

        return Ok(new { token });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirst("sub")?.Value;

        if (userId == null)
            return Unauthorized();

        var user = await _db.Users.FindAsync(Guid.Parse(userId));

        return Ok(new
        {
            user!.Id,
            user.FullName,
            user.Email
        });
    }
}
