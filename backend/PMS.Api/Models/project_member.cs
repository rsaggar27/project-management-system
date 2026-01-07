namespace PMS.Api.Models;

public class ProjectMember
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public ProjectRole Role { get; set; }
}
