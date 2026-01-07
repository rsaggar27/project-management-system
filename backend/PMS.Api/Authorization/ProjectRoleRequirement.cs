using Microsoft.AspNetCore.Authorization;
using PMS.Api.Models;

namespace PMS.Api.Authorization;

public class ProjectRoleRequirement : IAuthorizationRequirement
{
    public ProjectRole RequiredRole { get; }

    public ProjectRoleRequirement(ProjectRole role)
    {
        RequiredRole = role;
    }
}
