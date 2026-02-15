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
    public class PhasesController : ControllerBase
    {
        private readonly IPhaseService _service;

        public PhasesController(IPhaseService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAllPhases()
        {

            var data = await _service.GetAllPhases();

            return Ok(new Response<IEnumerable<PhaseResponseDto>>(data));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPhaseById([FromRoute][Range(1, int.MaxValue, ErrorMessage = "ID inválido")] int id)
        {

            var data = await _service.GetPhaseById(id);

            if (data == null)
                return NotFound(new Response<string>("La fase solicitado no existe en PatoCup."));

            return Ok(new Response<PhaseResponseDto>(data));
        }

        [HttpPost]
        [AuditLog("CREATE_PHASE")]
        public async Task<IActionResult> CreatePhase([FromBody] CreatePhaseDto dto)
        {
            var newId = await _service.CreatePhase(dto);

            return CreatedAtAction(
                nameof(GetPhaseById),
                new { id = newId },
                new Response<int>(newId, "¡fase creada con éxito!")
            );

        }

        [HttpPut("{id:int}")]
        [AuditLog("UPDATE_PHASE")]
        public async Task<IActionResult> UpdatePhase([FromRoute][Range(1, int.MaxValue)] int id, [FromBody] UpdatePhaseDto dto)
        {

            dto.Id = id;

            var updated = await _service.UpdatePhase(dto);

            if (!updated)
                return NotFound(new Response<string>("No se pudo actualizar porque la fase no existe."));

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [AuditLog("DELETE_PHASE")]
        public async Task<IActionResult> DeletePhase([FromRoute][Range(1, int.MaxValue)] int id)
        {

            var deleted = await _service.SoftDeletePhase(id);

            if (!deleted)
                return NotFound(new Response<string>("El fase no existe o ya fue eliminado."));

            return NoContent();
        }

        [HttpPatch("{id:int}/entity")]
        [AuditLog("REACTIVATE_PHASE")]
        public async Task<IActionResult> ReactivatePhase([FromRoute][Range(1, int.MaxValue)] int id)
        {

            var reactivated = await _service.ReactivatePhase(id);

            if (!reactivated)
                return Conflict(new Response<string>("La fase ya está activa o no existe."));

            return Ok(new Response<bool>(true, "La fase ha sido reactivado satisfactoriamente."));
        }

        [HttpGet("tournament/{tournamentId:int}")]
        public async Task<IActionResult> GetPhasesByTournamentId([FromRoute] int tournamentId)
        {
            var data = await _service.GetPhasesByTournamentId(tournamentId);

            return Ok(new Response<IEnumerable<PhaseResponseDto>>(data ?? new List<PhaseResponseDto>()));
        }

        [HttpPatch("tournament/{tournamentId:int}/final/{phaseId:int}")]
        [AuditLog("SET_FINAL_PHASE")]
        public async Task<IActionResult> SetFinalPhase(
            [FromRoute][Range(1, int.MaxValue)] int tournamentId,
            [FromRoute][Range(1, int.MaxValue)] int phaseId)
        {
            var result = await _service.SetFinalPhase(tournamentId, phaseId);

            return Ok(new Response<bool>(result, "¡Fase final de la PatoCup establecida con éxito!"));
        }
    }
}
