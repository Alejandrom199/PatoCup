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
    public class MatchesController : ControllerBase
    {
        private readonly IMatchService _service;

        public MatchesController(IMatchService service) => _service = service;

        [HttpGet("phase/{phaseId:int}")]
        public async Task<IActionResult> GetByPhase([FromRoute][Range(1, int.MaxValue)] int phaseId)
        {
            var data = await _service.GetMatchesByPhaseId(phaseId);

            return Ok(new Response<IEnumerable<MatchResponseDto>>(data));
        }

        [HttpPost]
        [AuditLog("CREATE_MATCH")]
        public async Task<IActionResult> CreateMatch([FromBody] CreateMatchDto dto)
        {
            var newId = await _service.CreateMatch(dto);

            return Ok(new Response<int>(newId, "Partida creada y programada exitosamente."));
        }

        [HttpPut("{id:int}")]
        [AuditLog("UPDATE_MATCH")]
        public async Task<IActionResult> UpdateMatch([FromRoute][Range(1, int.MaxValue)] int id, [FromBody] UpdateMatchDto dto)
        {
            dto.Id = id;
            var updated = await _service.UpdateMatch(dto);

            if (!updated)
            {
                return NotFound(new Response<string>("No se pudo actualizar la partida. Verifica que exista."));
            }

            return NoContent();
        }

        [HttpPut("result")]
        [AuditLog("REGISTER_MATCH_RESULT")]
        public async Task<IActionResult> RegisterResult([FromBody] RegisterMatchResultDto dto)
        {
            var success = await _service.RegisterResult(dto);

            if (!success)
            {
                return NotFound(new Response<string>("No se pudo registrar el resultado. Verifica que la partida exista."));
            }

            return Ok(new Response<string>(null, "Marcador registrado y estado actualizado."));
        }

        [HttpDelete("{id:int}")]
        [AuditLog("DELETE_MATCH")]
        public async Task<IActionResult> Delete([FromRoute][Range(1, int.MaxValue)] int id)
        {
            var deleted = await _service.SoftDeleteMatch(id);

            if (!deleted)
                return NotFound(new Response<string>("La partida no existe o ya fue eliminada."));

            return NoContent();
        }
    }
}