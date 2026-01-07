using PMS.Api.Models;

namespace PMS.Api.DTOs;

public class WorkspaceMemberResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public WorkspaceRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}
