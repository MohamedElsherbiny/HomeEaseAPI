namespace HomeEase.Application.DTOs.Auth
{
    public class RegisterProviderResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
    }
}
