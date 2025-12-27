namespace PMS.Api.Models;

public class ActivityLog
{
    public Guid Id { get; set; }

    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;

    public Guid ActorId { get; set; }
    public User Actor { get; set; } = null!;

    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }

    public string Action { get; set; } = null!;
    public string? MetadataJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
