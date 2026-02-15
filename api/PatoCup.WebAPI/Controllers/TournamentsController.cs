using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatoCup.Application.DTOs.Common;
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
    public class TournamentsController : ControllerBase
    {
        private readonly ITournamentService _service;

        public TournamentsController(ITournamentService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAllTournaments([FromQuery] TournamentQueryDto query)
        {

            var data = await _service.GetAllTournaments(query);

            return Ok(new Response<IEnumerable<TournamentResponseDto>>(data));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTournamentById([FromRoute][Range(1, int.MaxValue, ErrorMessage = "ID inválido")] int id)
        {

            var data = await _service.GetTournamentById(id);

            if (data == null) 
                return NotFound(new Response<string>("El torneo solicitado no existe en PatoCup."));

            return Ok(new Response<TournamentResponseDto>(data));
        }

        [HttpPost]
        [AuditLog("CREATE_TOURNAMENT")]
        public async Task<IActionResult> CreateTournament([FromBody] CreateTournamentDto dto)
        {
            var newId = await _service.CreateTournament(dto);

            return CreatedAtAction(
                nameof(GetTournamentById), 
                new { id = newId }, 
                new Response<int>(newId, "¡Torneo creado con éxito!")
            );

        }

        [HttpPut("{id:int}")]
        [AuditLog("UPDATE_TOURNAMENT")]
        public async Task<IActionResult> UpdateTournament([FromRoute][Range(1, int.MaxValue)] int id, [FromBody] UpdateTournamentDto dto)
        {

            dto.Id = id;

            var updated = await _service.UpdateTournament(dto);

            if (!updated) 
                    return NotFound(new Response<string>("No se pudo actualizar porque el torneo no existe."));

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [AuditLog("DELETE_TOURNAMENT")]
        public async Task<IActionResult> DeleteTournament([FromRoute][Range(1, int.MaxValue)] int id)
        {
            
            var deleted = await _service.SoftDeleteTournament(id);

            if (!deleted) 
                return NotFound(new Response<string>("El torneo no existe o ya fue eliminado."));

            return NoContent();
        }

        [HttpPatch("{id:int}/entity")]
        [AuditLog("REACTIVATE_TOURNAMENT")]
        public async Task<IActionResult> ReactivateTournament([FromRoute][Range(1, int.MaxValue)] int id)
        {

            var reactivated = await _service.ReactivateTournament(id);

            if (!reactivated)
                return Conflict(new Response<string>("El torneo ya está activo o no existe."));

            return Ok(new Response<bool>(true, "El torneo ha sido reactivado satisfactoriamente."));
        }

        [HttpPut("{id:int}/set-public")]
        [AuditLog("SET_PUBLIC_TOURNAMENT")]
        public async Task<IActionResult> SetPublicTournament([FromRoute][Range(1, int.MaxValue)] int id)
        {
            var isSuccess = await _service.SetPublicTournament(id);

            if (!isSuccess)
                return Conflict(new Response<string>("El torneo pudo ser seleccionado como público."));

            return Ok(new Response<bool>(true, "El torneo se ha seleccionado como público."));
        }

        [HttpGet("public")]
        public async Task<IActionResult> GetPublicActiveTournament()
        {
            var data = await _service.GetPublicActiveTournament();

            if (data == null)
                return NotFound(new Response<string>("No se ha obtenido torneo público."));

            return Ok(new Response<TournamentResponseDto>(data));
        }

        [HttpGet("public-bracket")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicBracket()
        {
            var data = await _service.GetPublicBracket();

            if (data == null)
                return NotFound(new Response<string>("No hay torneos públicos configurados."));

            return Ok(new Response<TournamentBracketDto>(data));
        }
    }
}