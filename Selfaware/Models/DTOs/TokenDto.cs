using System.ComponentModel.DataAnnotations;

namespace Selfaware.Models.DTOs
{
    public class TokenDto
    {
        [Required]
        public string AccessToken { get; set; } = string.Empty;

       
        public string RefreshToken { get; set; } = string.Empty;
    }
}
