using PruebaOben.Domain.Entities;

namespace PruebaOben.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task<(IReadOnlyList<AuditLog> Items, long TotalCount)> GetPageAsync(
        long offset,
        int pageSize,
        string? action);
}
