using System.ComponentModel.DataAnnotations;
using PMS.Api.Models;

namespace PMS.Api.DTOs;

public class AddWorkspaceMemberRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public WorkspaceRole Role { get; set; }
}
