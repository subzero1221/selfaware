using System.ComponentModel.DataAnnotations;

namespace Selfaware.Models.DTOs
{
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
