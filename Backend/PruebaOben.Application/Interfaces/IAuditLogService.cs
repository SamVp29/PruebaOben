using PruebaOben.Application.DTOs;

namespace PruebaOben.Application.Interfaces;

public interface IAuditLogService
{
    Task<AuditLogPageDto> GetPageAsync(int page, int pageSize, string? action);
}
