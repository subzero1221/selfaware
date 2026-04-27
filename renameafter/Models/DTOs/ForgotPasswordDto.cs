using System.ComponentModel.DataAnnotations;

namespace renameafter.Models.DTOs
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

    }
}
