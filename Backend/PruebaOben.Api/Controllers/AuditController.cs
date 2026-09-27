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
    private static readonly string[] AllowedActions = ["INSERT", "UPDATE", "DELETE"];

    private readonly IAuditLogService _auditLogService;

    public AuditController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<AuditLogPageDto>> GetPage(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? action = null)
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

        action = string.IsNullOrWhiteSpace(action)
            ? null
            : action.Trim().ToUpperInvariant();
        if (action is not null && !AllowedActions.Contains(action, StringComparer.Ordinal))
        {
            return BadRequest(new
            {
                message = $"action debe ser uno de estos valores: {string.Join(", ", AllowedActions)}."
            });
        }

        var auditLogs = await _auditLogService.GetPageAsync(page, pageSize, action);
        return Ok(auditLogs);
    }
}
