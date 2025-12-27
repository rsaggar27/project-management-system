namespace PMS.Api.Models;

public class WorkspaceMember
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;

    public WorkspaceRole Role { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
