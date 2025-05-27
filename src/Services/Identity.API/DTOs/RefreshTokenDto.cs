using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "O refresh token é obrigatório")]
        public string RefreshToken { get; set; } = string.Empty;
    }
} 