using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Api.Data;
using PMS.Api.DTOs;
using PMS.Api.Models;
using System.Security.Claims;
using TaskStatus = PMS.Api.Models.TaskStatus;

namespace PMS.Api.Controllers;

[ApiController]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    private Guid UserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // CREATE TASK (Lead)
    [Authorize(Policy = "ProjectLead")]
    [HttpPost("api/projects/{projectId}/tasks")]
    public async Task<IActionResult> Create(
        Guid projectId,
        CreateTaskRequest req)
    {
        var task = new TaskItem
        {
            Title = req.Title,
            Description = req.Description,
            Priority = req.Priority,
            DueDate = req.DueDate,
            ProjectId = projectId,
            CreatedById = UserId
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return Ok(task.Id);
    }

    // UPDATE TASK METADATA (Lead)
    [Authorize(Policy = "ProjectLead")]
    [HttpPut("api/tasks/{taskId}")]
    public async Task<IActionResult> Update(
        Guid taskId,
        UpdateTaskRequest req)
    {
        var task = await _db.Tasks.FindAsync(taskId);
        if (task == null) return NotFound();

        task.Title = req.Title;
        task.Description = req.Description;
        task.Priority = req.Priority;
        task.DueDate = req.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok();
    }

    // ASSIGN TASK (Lead only)
    [Authorize(Policy = "ProjectLead")]
    [HttpPut("api/tasks/{taskId}/assign")]
    public async Task<IActionResult> AssignTask(
        Guid taskId,
        AssignTaskRequest req)
    {
        var task = await _db.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            return NotFound("Task not found");

        // Is the assignee part of the project?
        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(pm =>
                pm.ProjectId == task.ProjectId &&
                pm.UserId == req.AssigneeId);

        if (member == null)
            return BadRequest("User is not a member of this project");

        // Optional: block assigning to viewers
        if (member.Role == ProjectRole.Viewer)
            return BadRequest("Cannot assign tasks to a viewer");

        task.AssigneeId = req.AssigneeId;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok();
    }


    // UPDATE STATUS (Contributor+ but controlled)
    [Authorize(Policy = "ProjectContributor")]
    [HttpPut("api/tasks/{taskId}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid taskId,
        TaskStatus status)
    {
        var task = await _db.Tasks.FindAsync(taskId);
        if (task == null) return NotFound();

        // Explicit transitions only
        var valid = task.Status switch
        {
            TaskStatus.Todo => status == TaskStatus.InProgress,
            TaskStatus.InProgress => status == TaskStatus.Review,
            TaskStatus.Review => status == TaskStatus.Done,
            _ => false
        };

        if (!valid)
            return BadRequest("Invalid status transition");

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok();
    }

    // LIST TASKS (Viewer)
    [Authorize(Policy = "ProjectViewer")]
    [HttpGet("api/projects/{projectId}/tasks")]
    public async Task<IActionResult> List(Guid projectId)
    {
        var tasks = await _db.Tasks
            .Where(t => t.ProjectId == projectId)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Status,
                t.Priority,
                t.AssigneeId,
                t.DueDate
            })
            .ToListAsync();

        return Ok(tasks);
    }


    // DELETE TASK (Lead only)
    [Authorize(Policy = "ProjectLead")]
    [HttpDelete("api/tasks/{taskId}")]
    public async Task<IActionResult> Delete(Guid taskId)
    {
        var task = await _db.Tasks.FindAsync(taskId);
        if (task == null)
            return NotFound("Task not found");

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();

        return NoContent();
    }

}
