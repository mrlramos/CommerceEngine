using Identity.API.DTOs;
using Identity.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IAuthService authService, ILogger<UsersController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os usuários
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao obter usuários");
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém um usuário por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao obter usuário {UserId}", id);
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém um usuário por email
        /// </summary>
        [HttpGet("by-email/{email}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            try
            {
                var user = await _authService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao obter usuário por email {Email}", email);
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Atualiza informações de um usuário
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDto userDto)
        {
            try
            {
                var success = await _authService.UpdateUserAsync(id, userDto);
                if (!success)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                return Ok(new { message = "Usuário atualizado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao atualizar usuário {UserId}", id);
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Desativa um usuário
        /// </summary>
        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            try
            {
                var success = await _authService.DeactivateUserAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                return Ok(new { message = "Usuário desativado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao desativar usuário {UserId}", id);
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Ativa um usuário
        /// </summary>
        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ActivateUser(string id)
        {
            try
            {
                var success = await _authService.ActivateUserAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                return Ok(new { message = "Usuário ativado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao ativar usuário {UserId}", id);
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Adiciona um usuário a uma role
        /// </summary>
        [HttpPost("{id}/roles/{roleName}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddUserToRole(string id, string roleName)
        {
            try
            {
                var success = await _authService.AddUserToRoleAsync(id, roleName);
                if (!success)
                {
                    return BadRequest(new { message = "Falha ao adicionar usuário à role" });
                }

                return Ok(new { message = $"Usuário adicionado à role {roleName} com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao adicionar usuário {UserId} à role {RoleName}", id, roleName);
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Remove um usuário de uma role
        /// </summary>
        [HttpDelete("{id}/roles/{roleName}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveUserFromRole(string id, string roleName)
        {
            try
            {
                var success = await _authService.RemoveUserFromRoleAsync(id, roleName);
                if (!success)
                {
                    return BadRequest(new { message = "Falha ao remover usuário da role" });
                }

                return Ok(new { message = $"Usuário removido da role {roleName} com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao remover usuário {UserId} da role {RoleName}", id, roleName);
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém as roles de um usuário
        /// </summary>
        [HttpGet("{id}/roles")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserRoles(string id)
        {
            try
            {
                var roles = await _authService.GetUserRolesAsync(id);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao obter roles do usuário {UserId}", id);
                return BadRequest(new { message = "Erro interno do servidor" });
            }
        }
    }
} 