using System.ComponentModel.DataAnnotations;
using PMS.Api.Models;

namespace PMS.Api.DTOs;

public class AddProjectMemberRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public ProjectRole Role { get; set; }
}
