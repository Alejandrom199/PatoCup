using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatoCup.Application.DTOs.Audit;
using PatoCup.Application.Interfaces.Services.Audit;
using PatoCup.Application.Wrappers; 
using System.ComponentModel.DataAnnotations;

namespace PatoCup.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLogs(
            [FromQuery][Range(1, int.MaxValue)] int pageNumber = 1,
            [FromQuery][Range(1, 100)] int pageSize = 20)
        {
            var logs = await _auditService.GetAllLogs(pageNumber, pageSize);

            return Ok(new Response<IEnumerable<AuditLogResponseDto>>(logs));
        }
    }
}