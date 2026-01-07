using System.ComponentModel.DataAnnotations;
using PMS.Api.Models;


namespace PMS.Api.DTOs;

public class CreateTaskCommentRequest
{
    [Required]
    public string Content { get; set; } = null!;
}

