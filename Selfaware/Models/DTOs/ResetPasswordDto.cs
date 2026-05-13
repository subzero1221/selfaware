using System.ComponentModel.DataAnnotations;

namespace Selfaware.Models.DTOs
{
    public class ResetPasswordDto
    {
      
        public string UserId { get; set; }

        
        public string Token { get; set; }

        
        public string NewPassword { get; set; }

       
        public string ConfirmPassword { get; set; }
    }
}
