using PMS.Api.Models;

namespace PMS.Api.DTOs;

public class ProjectMemberResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public ProjectRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}
