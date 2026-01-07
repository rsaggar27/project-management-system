using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Api.Data;
using PMS.Api.DTOs;
using PMS.Api.Models;
using System.Security.Claims;

namespace PMS.Api.Controllers;

[ApiController]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProjectsController(AppDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId()
    {
        return Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );
    }

    // ---------------------------------
    // CREATE PROJECT (Manager+)
    // ---------------------------------
    [Authorize(Policy = "WorkspaceManager")]
    [HttpPost("api/workspaces/{workspaceId}/projects")]
    public async Task<IActionResult> Create(
        Guid workspaceId,
        CreateProjectRequest req)
    {
        var userId = GetUserId();

        var project = new Project
        {
            Name = req.Name,
            Description = req.Description,
            WorkspaceId = workspaceId,
            CreatedById = userId
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            project.Id,
            project.Name,
            project.Description
        });
    }

    // ---------------------------------
    // LIST PROJECTS (Member+)
    // ---------------------------------
    [Authorize(Policy = "WorkspaceMember")]
    [HttpGet("api/workspaces/{workspaceId}/projects")]
    public async Task<IActionResult> List(Guid workspaceId)
    {
        var projects = await _db.Projects
            .Where(p => p.WorkspaceId == workspaceId)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Status
            })
            .ToListAsync();

        return Ok(projects);
    }

    // ---------------------------------
    // UPDATE PROJECT (Manager+)
    // ---------------------------------
    [Authorize(Policy = "WorkspaceManager")]
    [HttpPut("api/workspaces/{workspaceId}/projects/{projectId}")]
    public async Task<IActionResult> Update(
        Guid projectId,
        UpdateProjectRequest req)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null)
            return NotFound();

        project.Name = req.Name;
        project.Description = req.Description;

        await _db.SaveChangesAsync();
        return Ok();
    }

    // ---------------------------------
    // DELETE PROJECT (Admin only)
    // ---------------------------------
    [Authorize(Policy = "WorkspaceAdmin")]
    [HttpDelete("/api/workspaces/{workspaceId}/projects/{projectId}")]
    public async Task<IActionResult> Delete(Guid projectId)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null)
            return NotFound();

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // ---------------------------------
    // LIST PROJECT MEMBERS
    // ---------------------------------
    [Authorize(Policy = "ProjectViewer")]
    [HttpGet("api/projects/{projectId}/members")]
    public async Task<IActionResult> GetMembers(Guid projectId)
    {
        var members = await _db.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Select(pm => new ProjectMemberResponse
            {
                UserId = pm.UserId,
                Email = pm.User.Email,
                Role = pm.Role,
                JoinedAt = pm.JoinedAt
            })
            .OrderBy(m => m.Role) // Lead → Contributor → Viewer
            .ToListAsync();

        return Ok(members);
    }

    // ---------------------------------
    // ADD PROJECT MEMBERS
    // ---------------------------------
    [Authorize(Policy = "WorkspaceManager")]
    [HttpPost("api/workspaces/{workspaceId}/projects/{projectId}/members")]
    public async Task<IActionResult> AddMember(
    Guid workspaceId, Guid projectId, AddProjectMemberRequest req)
    {
        // Ensure project exists
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null)
            return NotFound();

        // Ensure user is workspace member
        var isWorkspaceMember = await _db.WorkspaceMembers.AnyAsync(wm =>
            wm.UserId == req.UserId &&
            wm.WorkspaceId == project.WorkspaceId);

        if (!isWorkspaceMember)
            return BadRequest("User is not a workspace member");

        // Upsert project member
        var member = await _db.ProjectMembers
            .FindAsync(req.UserId, projectId);

        if (member == null)
        {
            member = new ProjectMember
            {
                UserId = req.UserId,
                ProjectId = projectId,
                Role = req.Role
            };
            _db.ProjectMembers.Add(member);
        }
        else
        {
            member.Role = req.Role;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }
    
    // ---------------------------------
    // UPDATE PROJECT MEMBER ROLE (Lead only)
    // ---------------------------------
    [Authorize(Policy = "ProjectLead")]
    [HttpPut("api/projects/{projectId}/members/{userId}/role")]
    public async Task<IActionResult> UpdateMemberRole(
        Guid projectId,
        Guid userId,
        UpdateProjectMemberRoleRequest req)
    {
        var currentUserId = GetUserId();

        // Prevent self-demotion
        if (currentUserId == userId)
            return BadRequest("You cannot change your own role");

        var member = await _db.ProjectMembers.FirstOrDefaultAsync(pm =>
            pm.ProjectId == projectId &&
            pm.UserId == userId);

        if (member == null)
            return NotFound("User is not a member of this project");

        member.Role = req.Role;

        await _db.SaveChangesAsync();
        return Ok();
    }


}
