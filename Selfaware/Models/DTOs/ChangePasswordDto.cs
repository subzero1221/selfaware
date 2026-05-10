using System.ComponentModel.DataAnnotations;

namespace Selfaware.Models.DTOs
{
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        [Required]
        [MinLength(10)]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
