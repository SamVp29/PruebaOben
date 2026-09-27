using PruebaOben.Application.DTOs;
using PruebaOben.Application.Interfaces;
using PruebaOben.Domain.Entities;
using PruebaOben.Domain.Interfaces;

namespace PruebaOben.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<AuditLogPageDto> GetPageAsync(int page, int pageSize, string? action)
    {
        var offset = ((long)page - 1) * pageSize;
        var (items, totalCount) = await _repository.GetPageAsync(offset, pageSize, action);

        return new AuditLogPageDto
        {
            page = page,
            pageSize = pageSize,
            totalCount = totalCount,
            items = items.Select(MapToResponse).ToArray()
        };
    }

    private static AuditLogResponseDto MapToResponse(AuditLog auditLog)
    {
        return new AuditLogResponseDto
        {
            id = auditLog.id,
            userId = auditLog.userId,
            affectedUsername = auditLog.affectedUsername,
            accion = auditLog.accion,
            entidad = auditLog.entidad,
            entidadId = auditLog.entidadId,
            nombreCampo = auditLog.nombreCampo,
            valorAnterior = auditLog.valorAnterior,
            valorNuevo = auditLog.valorNuevo,
            cambioRealizado = auditLog.cambioRealizado,
            actorUsername = auditLog.actorUsername,
            cambioAt = auditLog.cambioAt
        };
    }
}
