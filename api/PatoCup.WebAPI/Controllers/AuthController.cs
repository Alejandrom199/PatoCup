using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatoCup.Application.DTOs.Auth;
using PatoCup.Application.DTOs.Security;
using PatoCup.Application.Interfaces.Services.Security;
using PatoCup.Application.UseCases.Security;
using PatoCup.Application.Wrappers;
using PatoCup.WebAPI.Filters;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PatoCup.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMenuService _menuService;

        public AuthController(IAuthService authService, IMenuService menuService)
        {
            _authService = authService;
            _menuService = menuService;
        }

        [HttpPost("login")]
        [AuditLog("USER_LOGIN")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var resultDto = await _authService.LoginAsync(request);

            if (resultDto == null)
            {
                return BadRequest(new Response<string>("Credenciales incorrectas"));
            }

            // Configuración de la Cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            Response.Cookies.Append("accessToken", resultDto.Token, cookieOptions);

            return Ok(new Response<dynamic>(new
            {
                resultDto.Id,
                resultDto.Username,
                resultDto.Role,
                resultDto.PhotoUrl
            }, "Autenticación exitosa"));

        }

        [HttpPost("logout")]
        [AuditLog("USER_LOGOUT")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("accessToken");
            return Ok(new Response<string>("Sesión cerrada"));
        }

        [HttpPost("change-password")]
        [Authorize(Roles = "Administrador")]
        [AuditLog("CHANGE_PASSWORD")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var resultDto = await _authService.ChangePasswordAsync(dto);

            if (resultDto == false)
            {
                return BadRequest(new Response<string>("No se pudo cambiar la contraseña"));
            }

            return Ok(new Response<dynamic>(true, "Cambio de contraseña exitosa"));
        }

        [HttpGet("menu")]
        [Authorize]
        public async Task<IActionResult> GetMyMenu()
        {
            var userIdString = User.FindFirst("uid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new Response<string>("No se pudo identificar al usuario."));
            }

            var menu = await _menuService.GetMenuByUser(userId);

            return Ok(new Response<IEnumerable<MenuResponseDto>>(menu));
        }
    }
}