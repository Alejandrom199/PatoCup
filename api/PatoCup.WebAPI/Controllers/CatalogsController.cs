using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatoCup.Application.DTOs.Common;
using PatoCup.Application.DTOs.Competition;
using PatoCup.Application.Interfaces.Services.Common;
using PatoCup.Application.Wrappers;

namespace PatoCup.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class CatalogsController : ControllerBase
    {
        private readonly ICatalogService _service;

        public CatalogsController(ICatalogService service) => _service = service;

        [HttpGet("general-status")]
        public async Task<IActionResult> GetGeneralStatus()
        {
            var data = await _service.GeneralStatusGetAll();
            return Ok(new Response<IEnumerable<CatalogDto>>(data));
        }

        [HttpGet("match-status")]
        public async Task<IActionResult> GetMatchStatus()
        {
            var data = await _service.MatchStatusGetAll();
            return Ok(new Response<IEnumerable<CatalogDto>>(data));
        }

        [HttpGet("phase-status")]
        public async Task<IActionResult> GetPhaseStatus()
        {
            var data = await _service.PhaseStatusGetAll();
            return Ok(new Response<IEnumerable<CatalogDto>>(data));
        }

        [HttpGet("player-status")]
        public async Task<IActionResult> GetPlayerStatus()
        {
            var data = await _service.PlayerStatusGetAll();
            return Ok(new Response<IEnumerable<CatalogDto>>(data));
        }

        [HttpGet("tournament-status")]
        public async Task<IActionResult> GetTournamentStatus()
        {
            var data = await _service.TournamentStatusGetAll();
            return Ok(new Response<IEnumerable<CatalogDto>>(data));
        }
    }
}