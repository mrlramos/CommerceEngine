using Identity.API.DTOs;
using Identity.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Identity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registra um novo usuário
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var response = await _authService.RegisterAsync(registerDto, ipAddress);
                
                _logger.LogInformation("Usuário {Email} registrado com sucesso", registerDto.Email);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Falha no registro: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno no registro");
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Autentica um usuário
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var response = await _authService.LoginAsync(loginDto, ipAddress);
                
                _logger.LogInformation("Login realizado com sucesso para {Email}", loginDto.Email);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Tentativa de login não autorizada: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno no login");
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Renova o token de acesso usando refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var response = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken, ipAddress);
                
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Refresh token inválido: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno no refresh token");
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Revoga um refresh token
        /// </summary>
        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var success = await _authService.RevokeTokenAsync(refreshTokenDto.RefreshToken, ipAddress);
                
                if (success)
                {
                    return Ok(new { message = "Token revogado com sucesso" });
                }
                
                return BadRequest(new { message = "Token inválido" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao revogar token");
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Altera a senha do usuário autenticado
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Usuário não encontrado" });
                }

                var success = await _authService.ChangePasswordAsync(userId, changePasswordDto);
                
                if (success)
                {
                    return Ok(new { message = "Senha alterada com sucesso" });
                }
                
                return BadRequest(new { message = "Falha ao alterar senha" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao alterar senha");
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém informações do usuário autenticado
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Usuário não encontrado" });
                }

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao obter usuário atual");
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Solicita reset de senha
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
                
                // Sempre retorna sucesso por segurança
                return Ok(new { message = "Se o email existir, um link de reset será enviado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno no forgot password");
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }
            
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }
} 