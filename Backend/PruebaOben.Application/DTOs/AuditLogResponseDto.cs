namespace PruebaOben.Application.DTOs;

public class AuditLogResponseDto
{
    public long id { get; set; }
    public int? userId { get; set; }
    public string? affectedUsername { get; set; }
    public string accion { get; set; } = string.Empty;
    public string entidad { get; set; } = string.Empty;
    public int? entidadId { get; set; }
    public string? nombreCampo { get; set; }
    public string? valorAnterior { get; set; }
    public string? valorNuevo { get; set; }
    public int? cambioRealizado { get; set; }
    public string? actorUsername { get; set; }
    public DateTime cambioAt { get; set; }
}
