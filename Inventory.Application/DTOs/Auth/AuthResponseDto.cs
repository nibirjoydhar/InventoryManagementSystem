namespace Inventory.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;

        public int ExpiresIn { get; set; } 
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
