using System.ComponentModel.DataAnnotations;
namespace PMS.Api.DTOs;

public class CreateWorkspaceRequest
{
    [Required]
    public string Name { get; set; } = null!;
}