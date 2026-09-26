using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PruebaOben.Application.DTOs;
using PruebaOben.Application.Interfaces;

namespace PruebaOben.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/audit")]
public class AuditController : ControllerBase
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 100;

    private readonly IAuditLogService _auditLogService;

    public AuditController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<AuditLogPageDto>> GetPage(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize)
    {
        if (page < 1)
        {
            return BadRequest(new { message = "page debe ser mayor o igual a 1." });
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            return BadRequest(new
            {
                message = $"pageSize debe estar entre 1 y {MaxPageSize}."
            });
        }

        var auditLogs = await _auditLogService.GetPageAsync(page, pageSize);
        return Ok(auditLogs);
    }
}
