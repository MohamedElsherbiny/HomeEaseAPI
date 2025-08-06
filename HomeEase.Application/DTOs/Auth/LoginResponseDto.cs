namespace HomeEase.Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public bool ProfileCompleted { get; set; }
        public string Role { get; set; }
    }
}
