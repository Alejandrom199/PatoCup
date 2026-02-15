using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatoCup.Application.DTOs.Security;
using PatoCup.Application.Interfaces.Services.Security;
using PatoCup.Application.Wrappers;
using PatoCup.WebAPI.Filters;
using System.ComponentModel.DataAnnotations;

namespace PatoCup.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("public")]
        [AllowAnonymous] 
        public IActionResult PublicInfo()
        {
            return Ok("Hola mundo");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery][Range(1, int.MaxValue)] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? username = null,
            [FromQuery] int? roleId = null)
        {
            var data = await _userService.GetAllAsync(pageNumber, pageSize, username, roleId);
            return Ok(new Response<IEnumerable<UserResponseDto>>(data));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute][Range(1, int.MaxValue)] int id)
        {
            var data = await _userService.GetByIdAsync(id);

            if (data == null)
                return NotFound(new Response<string>("Usuario no encontrado."));

            return Ok(new Response<UserResponseDto>(data));
        }

        [HttpPost]
        [AuditLog("CREATE_USER")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto request)
        {
            var newId = await _userService.CreateUserAsync(request);

            return Ok(new Response<int>(newId, "Usuario creado exitosamente"));
        }

        [HttpPut("{id:int}")]
        [AuditLog("UPDATE_USER")]
        public async Task<IActionResult> Update([FromRoute][Range(1, int.MaxValue)] int id, [FromBody] UpdateUserDto request)
        {
            if (id != request.Id)
                return BadRequest(new Response<string>("El ID de la ruta no coincide con el cuerpo de la petición."));

            var updated = await _userService.UpdateAsync(request);

            if (!updated)
                return NotFound(new Response<string>("No se pudo actualizar. El usuario no existe."));

            return Ok(new Response<string>(null, "Usuario actualizado correctamente."));
        }

        [HttpDelete("{id:int}")]
        [AuditLog("DELETE_USER")] 
        public async Task<IActionResult> Delete([FromRoute][Range(1, int.MaxValue)] int id)
        {
            var deleted = await _userService.SoftDeleteAsync(id);

            if (!deleted)
                return NotFound(new Response<string>("El usuario no existe o ya fue eliminado."));

            return NoContent(); 
        }
    }
}