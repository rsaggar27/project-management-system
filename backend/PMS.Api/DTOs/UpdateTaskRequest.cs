using System.ComponentModel.DataAnnotations;
using PMS.Api.Models;

namespace PMS.Api.DTOs;

public class UpdateTaskRequest
{
    [Required]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
}
