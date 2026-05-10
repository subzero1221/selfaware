namespace Selfaware.Models.DTOs
{
    public class SignupDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? ZodiacSign { get; set; }
    }
}
