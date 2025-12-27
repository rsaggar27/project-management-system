using Microsoft.AspNetCore.Authorization;
using PMS.Api.Models;

namespace PMS.Api.Authorization;

public class WorkspaceRoleRequirement : IAuthorizationRequirement
{
    public WorkspaceRole RequiredRole { get; }

    public WorkspaceRoleRequirement(WorkspaceRole requiredRole)
    {
        RequiredRole = requiredRole;
    }
}
