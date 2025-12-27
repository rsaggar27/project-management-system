using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Api.Data;
using PMS.Api.DTOs;
using PMS.Api.Models;
using System.Security.Claims;

namespace PMS.Api.Controllers;

[ApiController]
[Route("api/workspaces")]
[Authorize]
public class WorkspacesController : ControllerBase
{
    private readonly AppDbContext _db;

    public WorkspacesController(AppDbContext db)
    {
        _db = db;
    }

    // Helper: get userId from JWT
    private Guid GetUserId()
    {
        var userId =
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub");

    if (userId == null)
        throw new UnauthorizedAccessException("User ID not found in token");

    return Guid.Parse(userId);
    }

    // ----------------------------
    // CREATE WORKSPACE
    // ----------------------------
    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkspaceRequest req)
    {
        var userId = GetUserId();

        var workspace = new Workspace
        {
            Name = req.Name,
            CreatedById = userId
        };

        var membership = new WorkspaceMember
        {
            UserId = userId,
            Workspace = workspace,
            Role = WorkspaceRole.Admin
        };

        _db.Workspaces.Add(workspace);
        _db.WorkspaceMembers.Add(membership);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            workspace.Id,
            workspace.Name
        });
    }

    // ----------------------------
    // LIST MY WORKSPACES
    // ----------------------------
    [HttpGet]
    public async Task<IActionResult> MyWorkspaces()
    {
        var userId = GetUserId();

        var workspaces = await _db.WorkspaceMembers
            .Where(wm => wm.UserId == userId)
            .Select(wm => new
            {
                wm.Workspace.Id,
                wm.Workspace.Name,
                Role = wm.Role.ToString()
            })
            .ToListAsync();

        return Ok(workspaces);
    }

    // ----------------------------
    // ADD MEMBER (ADMIN ONLY)
    // ----------------------------
    [Authorize(Policy = "WorkspaceAdmin")]
    [HttpPost("{workspaceId}/members")]
    public async Task<IActionResult> AddMember(
        Guid workspaceId,
        AddWorkspaceMemberRequest req)
    {
        var userId = GetUserId();

        // Check requester role
        var requester = await _db.WorkspaceMembers
            .FirstOrDefaultAsync(wm =>
                wm.WorkspaceId == workspaceId &&
                wm.UserId == userId);

        if (requester == null)
            return Forbid();

        if (requester.Role != WorkspaceRole.Admin)
            return Forbid("Only Admin can add members");

        // Find user to add
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == req.Email);

        if (user == null)
            return NotFound("User not found");

        // Prevent duplicates
        var exists = await _db.WorkspaceMembers
            .AnyAsync(wm =>
                wm.WorkspaceId == workspaceId &&
                wm.UserId == user.Id);

        if (exists)
            return BadRequest("User already in workspace");

        var member = new WorkspaceMember
        {
            WorkspaceId = workspaceId,
            UserId = user.Id,
            Role = req.Role
        };

        _db.WorkspaceMembers.Add(member);
        await _db.SaveChangesAsync();

        return Ok();
    }
}
