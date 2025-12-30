using System.ComponentModel.DataAnnotations;

namespace PMS.Api.DTOs;

public class CreateProjectRequest
{
    [Required]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
