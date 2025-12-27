using System.ComponentModel.DataAnnotations;

namespace PMS.Api.Models;

public class Workspace
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
