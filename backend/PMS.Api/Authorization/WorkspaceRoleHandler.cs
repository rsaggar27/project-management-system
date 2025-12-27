using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PMS.Api.Data;
using PMS.Api.Models;
using System.Security.Claims;

namespace PMS.Api.Authorization;

public class WorkspaceRoleHandler
    : AuthorizationHandler<WorkspaceRoleRequirement>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public WorkspaceRoleHandler(
        AppDbContext db,
        IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorkspaceRoleRequirement requirement)
    {
        var userId = context.User
            .FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirst("sub")?.Value;

        if (userId == null)
            return;

        // Get workspaceId from route
        var routeValues = _http.HttpContext?.Request.RouteValues;
        if (routeValues == null || !routeValues.ContainsKey("workspaceId"))
            return;

        var workspaceId = Guid.Parse(routeValues["workspaceId"]!.ToString()!);

        var member = await _db.WorkspaceMembers.FirstOrDefaultAsync(wm =>
            wm.UserId == Guid.Parse(userId) &&
            wm.WorkspaceId == workspaceId);

        if (member == null)
            return;

        if (member.Role <= requirement.RequiredRole)
            context.Succeed(requirement);
    }
}
