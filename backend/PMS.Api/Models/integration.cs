namespace PMS.Api.Models;

public class Integration
{
    public Guid Id { get; set; }

    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;

    public string Provider { get; set; } = null!;
    public string AccessToken { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
