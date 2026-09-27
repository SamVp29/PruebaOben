using System.ComponentModel.DataAnnotations;

namespace PruebaOben.Shared.Services;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    public string email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string password { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public string token { get; set; } = string.Empty;
}

public sealed class UserDto
{
    public int id { get; set; }
    public string username { get; set; } = string.Empty;
    public string fullname { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string rol { get; set; } = string.Empty;
    public bool active { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime? updatedAt { get; set; }
    public DateTime? deletedAt { get; set; }
}

public sealed class CreateUserRequest
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre de usuario no puede superar 50 caracteres.")]
    public string username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre completo no puede superar 150 caracteres.")]
    public string fullname { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
    public string email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [StringLength(50, ErrorMessage = "El rol no puede superar 50 caracteres.")]
    public string rol { get; set; } = "User";
}

public sealed class UpdateUserRequest
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre de usuario no puede superar 50 caracteres.")]
    public string username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre completo no puede superar 150 caracteres.")]
    public string fullname { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
    public string email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [StringLength(50, ErrorMessage = "El rol no puede superar 50 caracteres.")]
    public string rol { get; set; } = "User";

    public bool active { get; set; }
}

public sealed class AuditLogDto
{
    public long id { get; set; }
    public int? userId { get; set; }
    public string accion { get; set; } = string.Empty;
    public string entidad { get; set; } = string.Empty;
    public int? entidadId { get; set; }
    public string? nombreCampo { get; set; }
    public string? valorAnterior { get; set; }
    public string? valorNuevo { get; set; }
    public int? cambioRealizado { get; set; }
    public DateTime cambioAt { get; set; }
}

public sealed class AuditPageDto
{
    public int page { get; set; }
    public int pageSize { get; set; }
    public long totalCount { get; set; }
    public IReadOnlyList<AuditLogDto> items { get; set; } = [];
}
