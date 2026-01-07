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
public class TaskCommentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TaskCommentsController(AppDbContext db)
    {
        _db = db;
    }

    private Guid UserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ---------------------------------
    // ADD COMMENT (Contributor+)
    // ---------------------------------
    [Authorize(Policy = "ProjectContributor")]
    [HttpPost("api/tasks/{taskId}/comments")]
    public async Task<IActionResult> Add(
        Guid taskId,
        CreateTaskCommentRequest req)
    {
        var taskExists = await _db.Tasks.AnyAsync(t => t.Id == taskId);
        if (!taskExists) return NotFound();

        var comment = new TaskComment
        {
            TaskId = taskId,
            Content = req.Content,
            UserId = UserId
        };

        _db.TaskComments.Add(comment);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            comment.Id,
            comment.Content,
            comment.CreatedAt
        });
    }

    // ---------------------------------
    // LIST COMMENTS (Viewer+)
    // ---------------------------------
    [Authorize(Policy = "ProjectViewer")]
    [HttpGet("api/tasks/{taskId}/comments")]
    public async Task<IActionResult> List(Guid taskId)
    {
        var comments = await _db.TaskComments
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.Content,
                c.CreatedAt,
                User = new
                {
                    c.User.Id,
                    c.User.Email
                }
            })
            .ToListAsync();

        return Ok(comments);
    }
}
