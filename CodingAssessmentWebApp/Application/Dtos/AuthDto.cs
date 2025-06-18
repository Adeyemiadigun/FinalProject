using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class LoginRequestModel
    {
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }

    }

    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public bool Status { get; set; }
    }
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }
   

}
