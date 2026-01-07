using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PMS.Api.Data;
using PMS.Api.Models;
using System.Security.Claims;

namespace PMS.Api.Authorization;

public class ProjectRoleHandler
    : AuthorizationHandler<ProjectRoleRequirement>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public ProjectRoleHandler(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectRoleRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return;

        var route = _http.HttpContext?.Request.RouteValues;
        if (route == null) return;

        Guid projectId = Guid.Empty;

        // 1. Try "projectId" from route
        if (route.TryGetValue("projectId", out var pVal) && pVal?.ToString() is string pStr)
        {
            Guid.TryParse(pStr, out projectId);
        }
        
        // 2. If not found, try "taskId" from route -> lookup ProjectId
        if (projectId == Guid.Empty && 
            route.TryGetValue("taskId", out var tVal) && 
            tVal?.ToString() is string tStr && 
            Guid.TryParse(tStr, out var taskId))
        {
            // We need to look up the project for this task
            // Using AsNoTracking for performance since we only need the ID
            var task = await _db.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task != null)
            {
                projectId = task.ProjectId;
            }
        }

        // If we still don't have a project ID, we can't authorize
        if (projectId == Guid.Empty) return;

        var member = await _db.ProjectMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(pm =>
                pm.ProjectId == projectId &&
                pm.UserId == Guid.Parse(userId));

        if (member == null) return;

        if (member.Role <= requirement.RequiredRole)
            context.Succeed(requirement);
    }
}
