using System.ComponentModel.DataAnnotations;

namespace PMS.Api.DTOs;

public class RegisterRequest
{
    [Required]
    public string FullName { get; set; } = null!;

    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required, MinLength(6)]
    public string Password { get; set; } = null!;
}
