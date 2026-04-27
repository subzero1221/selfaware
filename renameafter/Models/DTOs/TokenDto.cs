using System.ComponentModel.DataAnnotations;

namespace renameafter.Models.DTOs
{
    public class TokenDto
    {
        [Required]
        public string AccessToken { get; set; } = string.Empty;

       
        public string RefreshToken { get; set; } = string.Empty;
    }
}
