using System;
using System.Collections.Generic;

namespace PruebaOben.Application.DTOs;

public class AuditLogPageDto
{
    public int page { get; set; }
    public int pageSize { get; set; }
    public long totalCount { get; set; }
    public IReadOnlyList<AuditLogResponseDto> items { get; set; } = Array.Empty<AuditLogResponseDto>();
}
