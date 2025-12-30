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
}
