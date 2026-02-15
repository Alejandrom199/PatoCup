using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatoCup.Application.DTOs.Competition;
using PatoCup.Application.Interfaces.Services.Competition;
using PatoCup.Application.Wrappers;
using PatoCup.WebAPI.Filters;
using System.ComponentModel.DataAnnotations;

namespace PatoCup.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService _service;

        public PlayersController(IPlayerService service) => _service = service;

        [HttpPost("public/submit")]
        [AllowAnonymous]
        [AuditLog("PUBLIC_PLAYER_SUBMIT")]
        public async Task<IActionResult> PublicSubmit([FromBody] PublicSubmitPlayerDto dto)
        {
            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            if (clientIp == "::1") clientIp = "127.0.0.1";

            var result = await _service.PublicSubmitPlayerAsync(dto, clientIp);

            return Ok(new Response<string>("¡Solicitud enviada con éxito! Espera a que el administrador te acepte."));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPlayers(
            [FromQuery][Range(1, int.MaxValue)] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10,
            [FromQuery] string? filter = null)
        {
            var data = await _service.GetAllPlayersAsync(pageNumber, pageSize, filter ?? string.Empty);

            return Ok(new Response<IEnumerable<PlayerResponseDto>>(data));
        }

        [HttpGet("select")]
        public async Task<IActionResult> GetPlayersSelect()
        {
            var data = await _service.GetPlayersSelect();
            return Ok(new Response<IEnumerable<PlayerSelectDto>>(data));
        }

        [HttpPatch("{id:int}/process")]
        [AuditLog("PROCESS_PLAYER_REQUEST")]
        public async Task<IActionResult> ProcessRequest(
            [FromRoute][Range(1, int.MaxValue)] int id,
            [FromQuery][Range(1, int.MaxValue)] int statusId)
        {
            var success = await _service.ProcessPlayerRequestAsync(id, statusId);

            if (!success)
                return NotFound(new Response<string>("No se pudo procesar la solicitud. Verifica el ID del jugador."));

            return Ok(new Response<string>(data: null, message: "Estado del jugador actualizado correctamente."));
        }

        [HttpPut("{id:int}")]
        [AuditLog("UPDATE_PLAYER")]
        public async Task<IActionResult> UpdatePlayer([FromRoute][Range(1, int.MaxValue)] int id, [FromBody] UpdatePlayerDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new Response<string>("El ID de la URL no coincide con el del cuerpo de la petición."));

            var updated = await _service.UpdatePlayerAsync(dto);

            if (!updated)
                return NotFound(new Response<string>("El jugador no existe o no se pudo actualizar."));

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [AuditLog("DELETE_PLAYER")]
        public async Task<IActionResult> DeletePlayer([FromRoute][Range(1, int.MaxValue)] int id)
        {
            var deleted = await _service.SoftDeletePlayerAsync(id);

            if (!deleted)
                return NotFound(new Response<string>("El jugador no existe o ya fue eliminado."));

            return NoContent();
        }
    }
}